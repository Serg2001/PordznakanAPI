/* =====================================================================
   CleanupPupilDuplicates.sql   (PordznakanAPI, table dbo.Pupils)

   Keeps ONE row per KtakPupilId and deletes the rest.
     - Kept row  : the one with the latest UpdatedAt (then latest CreatedAt, then Id).
                   This is the row the sync was updating.
     - HasBook   : if ANY duplicate had HasBook = 1, the kept row gets HasBook = 1
                   (HasBook is not delivered by EMIS, so it must not be lost).
     - Backup    : every row of every duplicated pupil is copied to
                   dbo.Pupils_dup_backup_20261112 before anything is deleted.

   HOW TO RUN
     0. Stop the API / Hangfire sync jobs. Take a full database backup.
     1. Run the script as it is (@Execute = 0). It only REPORTS what it would do.
     2. Read the report. Then set @Execute = 1 and run again.
     3. After it prints "DONE", deploy the UniquePupilKtakId migration.

   Safe to re-run: if no duplicates remain it does nothing.
   ===================================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Execute   bit = 0;        -- 0 = report only, 1 = really change data
DECLARE @BatchSize int = 50000;    -- rows deleted per transaction

/* ---------- 1. Rank the rows of every duplicated pupil ---------- */
IF OBJECT_ID('tempdb..#Dups') IS NOT NULL DROP TABLE #Dups;

;WITH r AS (
    SELECT Id,
           KtakPupilId,
           HasBook,
           KtakSchoolId,
           RegionId,
           ROW_NUMBER() OVER (PARTITION BY KtakPupilId
                              ORDER BY UpdatedAt DESC, CreatedAt DESC, Id) AS rn,
           MAX(CAST(HasBook AS int)) OVER (PARTITION BY KtakPupilId)        AS AnyBook,
           COUNT(*)                  OVER (PARTITION BY KtakPupilId)        AS Cnt
    FROM dbo.Pupils
)
SELECT Id, KtakPupilId, HasBook, KtakSchoolId, RegionId, rn, AnyBook, Cnt
INTO #Dups
FROM r
WHERE Cnt > 1;

CREATE UNIQUE CLUSTERED INDEX IX_Dups_Id ON #Dups (Id);

/* ---------- 2. Report ---------- */
DECLARE @TotalRowsBefore      int = (SELECT COUNT(*) FROM dbo.Pupils);
DECLARE @DistinctPupilsBefore int = (SELECT COUNT(DISTINCT KtakPupilId) FROM dbo.Pupils);
DECLARE @RowsToDelete         int = (SELECT COUNT(*) FROM #Dups WHERE rn > 1);

SELECT @TotalRowsBefore                                              AS TotalRows,
       @DistinctPupilsBefore                                         AS DistinctPupils,
       (SELECT COUNT(DISTINCT KtakPupilId) FROM #Dups)               AS PupilsWithDuplicates,
       @RowsToDelete                                                 AS RowsToDelete,
       (SELECT COUNT(*) FROM #Dups
        WHERE rn = 1 AND AnyBook = 1 AND HasBook = 0)                AS HasBookToCarryOver,
       (SELECT COUNT(*) FROM (SELECT KtakPupilId FROM #Dups
                              GROUP BY KtakPupilId
                              HAVING COUNT(DISTINCT KtakSchoolId) > 1
                                  OR COUNT(DISTINCT RegionId) > 1) x) AS PupilsWithDifferentSchoolOrRegion;

-- How many rows do duplicated pupils have? (2 = one extra copy, 4 = special schools, ...)
SELECT Cnt AS RowsPerPupil, COUNT(DISTINCT KtakPupilId) AS Pupils
FROM #Dups
GROUP BY Cnt
ORDER BY Cnt;

IF @RowsToDelete = 0
BEGIN
    PRINT 'No duplicates found. Nothing to do.';
    RETURN;
END

IF @Execute = 0
BEGIN
    PRINT 'REPORT ONLY. Nothing was changed. Set @Execute = 1 to clean up.';
    RETURN;
END

/* ---------- 3. Backup every row of every duplicated pupil ---------- */
IF OBJECT_ID('dbo.Pupils_dup_backup_20261112') IS NOT NULL
    THROW 50001, 'dbo.Pupils_dup_backup_20261112 already exists. Check it, rename or drop it, then run again.', 1;

SELECT p.*
INTO dbo.Pupils_dup_backup_20261112
FROM dbo.Pupils p
JOIN #Dups d ON d.Id = p.Id;

PRINT 'Backup table created: dbo.Pupils_dup_backup_20261112';

/* ---------- 4. Carry HasBook over to the kept row ---------- */
BEGIN TRAN;

UPDATE p
SET    p.HasBook = 1
FROM   dbo.Pupils p
JOIN   #Dups d ON d.Id = p.Id
WHERE  d.rn = 1 AND d.AnyBook = 1 AND d.HasBook = 0;

PRINT CONCAT('HasBook carried over for ', @@ROWCOUNT, ' pupils.');
COMMIT;

/* ---------- 5. Delete the extra rows in batches ---------- */
IF OBJECT_ID('tempdb..#ToDelete') IS NOT NULL DROP TABLE #ToDelete;

SELECT Id INTO #ToDelete FROM #Dups WHERE rn > 1;
CREATE UNIQUE CLUSTERED INDEX IX_ToDelete_Id ON #ToDelete (Id);

DECLARE @Deleted int = 1, @TotalDeleted int = 0;

WHILE @Deleted > 0
BEGIN
    BEGIN TRAN;

    DELETE TOP (@BatchSize) p
    FROM dbo.Pupils p
    JOIN #ToDelete t ON t.Id = p.Id;

    SET @Deleted = @@ROWCOUNT;
    COMMIT;

    SET @TotalDeleted += @Deleted;
    IF @Deleted > 0
        RAISERROR('Deleted %d rows so far...', 0, 1, @TotalDeleted) WITH NOWAIT;
END

/* ---------- 6. Verify ---------- */
DECLARE @TotalRowsAfter      int = (SELECT COUNT(*) FROM dbo.Pupils);
DECLARE @DistinctPupilsAfter int = (SELECT COUNT(DISTINCT KtakPupilId) FROM dbo.Pupils);
DECLARE @StillDuplicated     int = (SELECT COUNT(*) FROM (SELECT KtakPupilId FROM dbo.Pupils
                                                          GROUP BY KtakPupilId
                                                          HAVING COUNT(*) > 1) x);

SELECT @TotalRowsBefore      AS TotalRowsBefore,
       @TotalRowsAfter       AS TotalRowsAfter,
       @TotalDeleted         AS RowsDeleted,
       @DistinctPupilsBefore AS DistinctPupilsBefore,
       @DistinctPupilsAfter  AS DistinctPupilsAfter,
       @StillDuplicated      AS StillDuplicatedPupils;

IF @DistinctPupilsAfter <> @DistinctPupilsBefore
    THROW 50002, 'Number of distinct pupils changed! Restore from dbo.Pupils_dup_backup_20261112 and investigate.', 1;

IF @StillDuplicated <> 0
    THROW 50003, 'Duplicates still exist (rows were added during the cleanup?). Stop the sync and run the script again.', 1;

PRINT 'DONE. Every pupil has exactly one row. You can now deploy the UniquePupilKtakId migration.';

/* ---------------------------------------------------------------------
   ROLLBACK (only if something is wrong). Restores the deleted rows:

   INSERT INTO dbo.Pupils
   SELECT b.* FROM dbo.Pupils_dup_backup_20261112 b
   WHERE NOT EXISTS (SELECT 1 FROM dbo.Pupils p WHERE p.Id = b.Id);

   (This brings the duplicates back, so the unique index could not be created.)
   Drop the backup table only after you have verified the application.
   --------------------------------------------------------------------- */
