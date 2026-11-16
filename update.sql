BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112112441_UniquePupilKtakId'
)
BEGIN
    DROP INDEX [IX_Pupils_KtakPupilId] ON [Pupils];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112112441_UniquePupilKtakId'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Pupils_KtakPupilId] ON [Pupils] ([KtakPupilId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261112112441_UniquePupilKtakId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261112112441_UniquePupilKtakId', N'9.0.11');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115203354_UniqueMuhAndEmployeeIndexes'
)
BEGIN
    DROP INDEX [IX_MuhStudents_MuhStudentId] ON [MuhStudents];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115203354_UniqueMuhAndEmployeeIndexes'
)
BEGIN
    DROP INDEX [IX_MuhInstitutions_InstitutionId] ON [MuhInstitutions];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115203354_UniqueMuhAndEmployeeIndexes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SchoolEmployees_RegionId_PersonId_VacationId] ON [SchoolEmployees] ([RegionId], [PersonId], [VacationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115203354_UniqueMuhAndEmployeeIndexes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MuhStudents_MuhStudentId] ON [MuhStudents] ([MuhStudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115203354_UniqueMuhAndEmployeeIndexes'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MuhInstitutions_InstitutionId] ON [MuhInstitutions] ([InstitutionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261115203354_UniqueMuhAndEmployeeIndexes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261115203354_UniqueMuhAndEmployeeIndexes', N'9.0.11');
END;

COMMIT;
GO

