/* =====================================================================
   CleanupPupilDuplicates_LowMemory.sql   (PordznakanAPI, table dbo.Pupils)

   Same result as CleanupPupilDuplicates.sql, but built for servers with little
   memory (error 802 "insufficient memory in the buffer pool", SQL Server Express):

     - Works in small KtakPupilId ranges (@Step ids at a time) instead of ranking the
       whole table at once. All rows of one pupil always fall in the same range.
     - MAXDOP 1, small temp table, one short transaction per range.
     - Resumable: if it stops half way, just run it again.

   WHAT IT DOES (per pupil that has more than one row)
     - Keeps ONE row: latest UpdatedAt (then latest CreatedAt, then Id).
     - If ANY duplicate had HasBook = 1, the kept row gets HasBook = 1.
     - Copies every row of that pupil to dbo.Pupils_dup_backup_20261112 BEFORE deleting.
     - Pupils with a single row and every other table are not touched.

   HOW TO RUN
     0. Stop the API / Hangfire sync jobs. Take a full database backup.
     1. Run as it is (@Execute = 0): report only, nothing is changed.
        Expected from your earlier report: TotalRows 574029, DistinctPupils 429268,
        PupilsWithDuplicates 142953, RowsToDelete 144761, HasBookToCarryOver 87828,
        PupilsWithDifferentSchoolOrRegion 34.
     2. Set @Execute = 1 and run again. It prints progress and "DONE" at the end.
     3. Then deploy the UniquePupilKtakId migration.

   Still error 802? Lower @Step (for example 5000) and run again.
   ===================================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Execute bit = 0;        -- 0 = report only, 1 = really change data
DECLARE @Step    int = 20000;    -- size of one KtakPupilId range; lower it if memory is tight

DECLARE @MinId int, @MaxId int;
SELECT @MinId = MIN(KtakPupilId), @MaxId = MAX(KtakPupilId) FROM dbo.Pupils;

IF @MinId IS NULL
BEGIN
    PRINT 'dbo.Pupils is empty. Nothing to do.';
    RETURN;
END

/* ---------- Working tables ---------- */
IF OBJECT_ID('tempdb..#C') IS NOT NULL DROP TABLE #C;
CREATE TABLE #C (
    Id           uniqueidentifier NOT NULL PRIMARY KEY,
    KtakPupilId  int NULL,
    HasBook      bit NULL,
    KtakSchoolId int NULL,
    RegionId     int NULL,
    rn           int NULL,
    AnyBook      int NULL,
    Cnt          int NULL
);

IF OBJECT_ID('tempdb..#Stats') IS NOT NULL DROP TABLE #Stats;
CREATE TABLE #Stats (Cnt int NOT NULL PRIMARY KEY, Pupils int NOT NULL);

IF @Execute = 1
BEGIN
    IF OBJECT_ID('dbo.Pupils_dup_backup_20261112') IS NULL
    BEGIN
        SELECT TOP (0) * INTO dbo.Pupils_dup_backup_20261112 FROM dbo.Pupils;
        PRINT 'Backup table created: dbo.Pupils_dup_backup_20261112';
    END
    ELSE
        PRINT 'Backup table dbo.Pupils_dup_backup_20261112 already exists: new rows will be appended.';
END

DECLARE @From int = @MinId, @To int;
DECLARE @r bigint, @d bigint, @n bigint, @Chunks int = 0;
DECLARE @TotalRowsBefore bigint = 0, @DistinctBefore bigint = 0,
        @PupilsWithDup bigint = 0, @RowsToDelete bigint = 0,
        @HasBookCarry bigint = 0, @DiffSchoolRegion bigint = 0,
        @Deleted bigint = 0;
DECLARE @pf int, @pt int;

/* ---------- Pass 1: find (and, if @Execute = 1, clean) range by range ---------- */
WHILE @From <= @MaxId
BEGIN
    SET @To = @From + @Step;            -- exclusive upper bound

    SELECT @r = COUNT_BIG(*), @d = COUNT_BIG(DISTINCT KtakPupilId)
    FROM dbo.Pupils
    WHERE KtakPupilId >= @From AND KtakPupilId < @To;

    SET @TotalRowsBefore += @r;
    SET @DistinctBefore  += @d;

    IF @r > @d        -- this range contains duplicates
    BEGIN
        TRUNCATE TABLE #C;

        ;WITH x AS (
            SELECT Id, KtakPupilId, HasBook, KtakSchoolId, RegionId,
                   ROW_NUMBER() OVER (PARTITION BY KtakPupilId
                                      ORDER BY UpdatedAt DESC, CreatedAt DESC, Id) AS rn,
                   MAX(CAST(HasBook AS int)) OVER (PARTITION BY KtakPupilId)        AS AnyBook,
                   COUNT(*)                  OVER (PARTITION BY KtakPupilId)        AS Cnt
            FROM dbo.Pupils
            WHERE KtakPupilId >= @From AND KtakPupilId < @To
        )
        INSERT INTO #C (Id, KtakPupilId, HasBook, KtakSchoolId, RegionId, rn, AnyBook, Cnt)
        SELECT Id, KtakPupilId, HasBook, KtakSchoolId, RegionId, rn, AnyBook, Cnt
        FROM x
        WHERE Cnt > 1
        OPTION (MAXDOP 1);

        -- statistics for the report
        SELECT @n = COUNT_BIG(*) FROM #C WHERE rn > 1;
        SET @RowsToDelete += @n;

        SELECT @n = COUNT_BIG(*) FROM #C WHERE rn = 1;
        SET @PupilsWithDup += @n;

        SELECT @n = COUNT_BIG(*) FROM #C WHERE rn = 1 AND AnyBook = 1 AND HasBook = 0;
        SET @HasBookCarry += @n;

        SELECT @n = COUNT_BIG(*)
        FROM (SELECT KtakPupilId FROM #C
              GROUP BY KtakPupilId
              HAVING COUNT(DISTINCT KtakSchoolId) > 1 OR COUNT(DISTINCT RegionId) > 1) z;
        SET @DiffSchoolRegion += @n;

        UPDATE s SET s.Pupils = s.Pupils + g.P
        FROM #Stats s
        JOIN (SELECT Cnt, COUNT(*) AS P FROM #C WHERE rn = 1 GROUP BY Cnt) g ON g.Cnt = s.Cnt;

        INSERT INTO #Stats (Cnt, Pupils)
        SELECT g.Cnt, g.P
        FROM (SELECT Cnt, COUNT(*) AS P FROM #C WHERE rn = 1 GROUP BY Cnt) g
        WHERE NOT EXISTS (SELECT 1 FROM #Stats s WHERE s.Cnt = g.Cnt);

        IF @Execute = 1
        BEGIN
            BEGIN TRAN;

            -- 1) backup every row of the duplicated pupils (skip rows already backed up)
            INSERT INTO dbo.Pupils_dup_backup_20261112
            SELECT p.*
            FROM dbo.Pupils p
            JOIN #C c ON c.Id = p.Id
            WHERE NOT EXISTS (SELECT 1 FROM dbo.Pupils_dup_backup_20261112 b WHERE b.Id = p.Id);

            -- 2) carry HasBook over to the kept row
            UPDATE p SET p.HasBook = 1
            FROM dbo.Pupils p
            JOIN #C c ON c.Id = p.Id
            WHERE c.rn = 1 AND c.AnyBook = 1 AND c.HasBook = 0;

            -- 3) delete the extra rows
            DELETE p
            FROM dbo.Pupils p
            JOIN #C c ON c.Id = p.Id
            WHERE c.rn > 1;

            SET @Deleted += @@ROWCOUNT;

            COMMIT;

            SET @Chunks += 1;
            IF @Chunks % 5 = 0
            BEGIN
                SET @pf = @From; SET @pt = @To;
                RAISERROR('Cleaned ids %d..%d. Deleted so far: %I64d rows.', 0, 1, @pf, @pt, @Deleted) WITH NOWAIT;
            END
        END
    END

    SET @From = @To;
END

/* ---------- Report ---------- */
SELECT @TotalRowsBefore  AS TotalRows,
       @DistinctBefore   AS DistinctPupils,
       @PupilsWithDup    AS PupilsWithDuplicates,
       @RowsToDelete     AS RowsToDelete,
       @HasBookCarry     AS HasBookToCarryOver,
       @DiffSchoolRegion AS PupilsWithDifferentSchoolOrRegion;

SELECT Cnt AS RowsPerPupil, Pupils FROM #Stats ORDER BY Cnt;

IF @Execute = 0
BEGIN
    PRINT 'REPORT ONLY. Nothing was changed. Set @Execute = 1 to clean up.';
    RETURN;
END

/* ---------- Pass 2: verify, again range by range ---------- */
DECLARE @RowsAfter bigint = 0, @DistinctAfter bigint = 0, @StillExtra bigint = 0;
SET @From = @MinId;

WHILE @From <= @MaxId
BEGIN
    SET @To = @From + @Step;

    SELECT @r = COUNT_BIG(*), @d = COUNT_BIG(DISTINCT KtakPupilId)
    FROM dbo.Pupils
    WHERE KtakPupilId >= @From AND KtakPupilId < @To;

    SET @RowsAfter     += @r;
    SET @DistinctAfter += @d;
    SET @StillExtra    += (@r - @d);

    SET @From = @To;
END

SELECT @TotalRowsBefore AS TotalRowsBefore,
       @RowsAfter       AS TotalRowsAfter,
       @Deleted         AS RowsDeleted,
       @DistinctBefore  AS DistinctPupilsBefore,
       @DistinctAfter   AS DistinctPupilsAfter,
       @StillExtra      AS ExtraRowsStillThere;

IF @DistinctAfter <> @DistinctBefore
    THROW 50002, 'Number of distinct pupils changed! Restore from dbo.Pupils_dup_backup_20261112 and investigate.', 1;

IF @StillExtra <> 0
    THROW 50003, 'Duplicates still exist (were rows added during the cleanup?). Stop the sync and run the script again.', 1;

PRINT 'DONE. Every pupil has exactly one row. You can now deploy the UniquePupilKtakId migration.';

/* ---------------------------------------------------------------------
   ROLLBACK (only if something is wrong). Restores the deleted rows:

   INSERT INTO dbo.Pupils
   SELECT b.* FROM dbo.Pupils_dup_backup_20261112 b
   WHERE NOT EXISTS (SELECT 1 FROM dbo.Pupils p WHERE p.Id = b.Id);

   (This brings the duplicates back, so the unique index could not be created.)
   Drop the backup table only after you have verified the application.
   --------------------------------------------------------------------- */
