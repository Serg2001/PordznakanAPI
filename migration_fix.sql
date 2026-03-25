IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [Employees] (
        [Id] int NOT NULL IDENTITY,
        [ExternalPersonId] int NOT NULL,
        [Ssn] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Position] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [MmuhInstitutions] (
        [Id] uniqueidentifier NOT NULL,
        [InstId] int NOT NULL,
        [RegionId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [LegalMarzId] nvarchar(max) NOT NULL,
        [LegalAddress] nvarchar(max) NOT NULL,
        [BusinessMarzId] nvarchar(max) NOT NULL,
        [BusinessAddress] nvarchar(max) NOT NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MmuhInstitutions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [MmuhInstitutionsStaging] (
        [Id] uniqueidentifier NOT NULL,
        [InstId] int NOT NULL,
        [RegionId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [LegalMarzId] nvarchar(max) NOT NULL,
        [LegalAddress] nvarchar(max) NOT NULL,
        [BusinessMarzId] nvarchar(max) NOT NULL,
        [BusinessAddress] nvarchar(max) NOT NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MmuhInstitutionsStaging] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [MmuhStaff] (
        [Id] uniqueidentifier NOT NULL,
        [MmuhStaffId] int NOT NULL,
        [InstId] int NOT NULL,
        [InternalSchoolId] uniqueidentifier NULL,
        [RegionId] int NOT NULL,
        [InstName] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [DateOfBirth] date NOT NULL,
        [SocNumber] nvarchar(max) NOT NULL,
        [Sex] bit NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Citizenship] nvarchar(max) NOT NULL,
        [Nationality] nvarchar(max) NOT NULL,
        [IdentDocument] nvarchar(max) NOT NULL,
        [IdentDocumentNumber] nvarchar(max) NOT NULL,
        [FromCountry] nvarchar(max) NOT NULL,
        [InFiz] nvarchar(max) NOT NULL,
        [Druyq] nvarchar(max) NOT NULL,
        [PartlyIds] nvarchar(max) NULL,
        [PartlyInstNames] nvarchar(max) NULL,
        [PositionName] nvarchar(max) NOT NULL,
        [PositionId] nvarchar(max) NOT NULL,
        [PositionDetailId] nvarchar(max) NOT NULL,
        [PositionDetailName] nvarchar(max) NOT NULL,
        [GroupIds] nvarchar(max) NOT NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MmuhStaff] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [MmuhStaffStaging] (
        [Id] uniqueidentifier NOT NULL,
        [MmuhStaffId] int NOT NULL,
        [InstId] int NOT NULL,
        [InternalSchoolId] uniqueidentifier NULL,
        [RegionId] int NOT NULL,
        [InstName] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [DateOfBirth] date NOT NULL,
        [SocNumber] nvarchar(max) NOT NULL,
        [Sex] bit NOT NULL,
        [SexRaw] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Citizenship] nvarchar(max) NOT NULL,
        [Nationality] nvarchar(max) NOT NULL,
        [IdentDocument] nvarchar(max) NOT NULL,
        [IdentDocumentNumber] nvarchar(max) NOT NULL,
        [FromCountry] nvarchar(max) NOT NULL,
        [InFiz] nvarchar(max) NOT NULL,
        [Druyq] nvarchar(max) NOT NULL,
        [PartlyIds] nvarchar(max) NULL,
        [PartlyInstNames] nvarchar(max) NULL,
        [PositionName] nvarchar(max) NOT NULL,
        [PositionId] nvarchar(max) NOT NULL,
        [PositionDetailId] nvarchar(max) NOT NULL,
        [PositionDetailName] nvarchar(max) NOT NULL,
        [GroupId] nvarchar(max) NOT NULL,
        [GroupsJson] nvarchar(max) NOT NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MmuhStaffStaging] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [MmuhStudents] (
        [Id] uniqueidentifier NOT NULL,
        [MmuhStudentId] int NOT NULL,
        [MmuhSchoolId] int NOT NULL,
        [InternalSchoolId] uniqueidentifier NULL,
        [RegionId] int NOT NULL,
        [SchoolName] nvarchar(max) NOT NULL,
        [Marz] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [DateOfBirth] date NOT NULL,
        [SocNumber] nvarchar(max) NOT NULL,
        [Sex] nvarchar(max) NOT NULL,
        [Graduated] bit NOT NULL,
        [GroupId] nvarchar(max) NOT NULL,
        [ClassroomGrade] int NOT NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MmuhStudents] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [MmuhStudentsStaging] (
        [Id] uniqueidentifier NOT NULL,
        [MmuhStudentId] int NOT NULL,
        [MmuhSchoolId] int NOT NULL,
        [InternalSchoolId] uniqueidentifier NULL,
        [RegionId] int NOT NULL,
        [SchoolName] nvarchar(max) NOT NULL,
        [Marz] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [DateOfBirth] date NOT NULL,
        [SocNumber] nvarchar(max) NOT NULL,
        [Sex] nvarchar(max) NOT NULL,
        [Graduated] bit NOT NULL,
        [GroupId] nvarchar(max) NOT NULL,
        [ClassroomGrade] int NOT NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MmuhStudentsStaging] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [NmuhInstitutions] (
        [Id] uniqueidentifier NOT NULL,
        [InstId] int NOT NULL,
        [RegionId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [LegalMarzId] nvarchar(max) NOT NULL,
        [LegalAddress] nvarchar(max) NOT NULL,
        [BusinessMarzId] nvarchar(max) NOT NULL,
        [BusinessAddress] nvarchar(max) NOT NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_NmuhInstitutions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [NmuhInstitutionsStaging] (
        [Id] uniqueidentifier NOT NULL,
        [InstId] int NOT NULL,
        [RegionId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [LegalMarzId] nvarchar(max) NOT NULL,
        [LegalAddress] nvarchar(max) NOT NULL,
        [BusinessMarzId] nvarchar(max) NOT NULL,
        [BusinessAddress] nvarchar(max) NOT NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_NmuhInstitutionsStaging] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [NmuhStaff] (
        [Id] uniqueidentifier NOT NULL,
        [NmuhStaffId] int NOT NULL,
        [InstId] int NOT NULL,
        [InternalSchoolId] uniqueidentifier NULL,
        [RegionId] int NOT NULL,
        [InstName] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [DateOfBirth] date NOT NULL,
        [SocNumber] nvarchar(max) NOT NULL,
        [Sex] bit NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Citizenship] nvarchar(max) NOT NULL,
        [Nationality] nvarchar(max) NOT NULL,
        [IdentDocument] nvarchar(max) NOT NULL,
        [IdentDocumentNumber] nvarchar(max) NOT NULL,
        [FromCountry] nvarchar(max) NOT NULL,
        [InFiz] nvarchar(max) NOT NULL,
        [Druyq] nvarchar(max) NOT NULL,
        [PartlyIds] nvarchar(max) NULL,
        [PartlyInstNames] nvarchar(max) NULL,
        [PositionName] nvarchar(max) NOT NULL,
        [PositionId] nvarchar(max) NOT NULL,
        [PositionDetailId] nvarchar(max) NOT NULL,
        [PositionDetailName] nvarchar(max) NOT NULL,
        [GroupIds] nvarchar(max) NOT NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_NmuhStaff] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [NmuhStaffStaging] (
        [Id] uniqueidentifier NOT NULL,
        [NmuhStaffId] int NOT NULL,
        [InstId] int NOT NULL,
        [InternalSchoolId] uniqueidentifier NULL,
        [RegionId] int NOT NULL,
        [InstName] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [DateOfBirth] date NOT NULL,
        [SocNumber] nvarchar(max) NOT NULL,
        [Sex] bit NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Citizenship] nvarchar(max) NOT NULL,
        [Nationality] nvarchar(max) NOT NULL,
        [IdentDocument] nvarchar(max) NOT NULL,
        [IdentDocumentNumber] nvarchar(max) NOT NULL,
        [FromCountry] nvarchar(max) NOT NULL,
        [InFiz] nvarchar(max) NOT NULL,
        [Druyq] nvarchar(max) NOT NULL,
        [PartlyIds] nvarchar(max) NULL,
        [PartlyInstNames] nvarchar(max) NULL,
        [PositionName] nvarchar(max) NOT NULL,
        [PositionId] nvarchar(max) NOT NULL,
        [PositionDetailId] nvarchar(max) NOT NULL,
        [PositionDetailName] nvarchar(max) NOT NULL,
        [GroupId] nvarchar(max) NULL,
        [GroupsJson] nvarchar(max) NOT NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_NmuhStaffStaging] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [NmuhStudents] (
        [Id] uniqueidentifier NOT NULL,
        [NmuhStudentId] int NOT NULL,
        [NmuhSchoolId] int NOT NULL,
        [InternalSchoolId] uniqueidentifier NULL,
        [RegionId] int NOT NULL,
        [SchoolName] nvarchar(max) NOT NULL,
        [Marz] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [DateOfBirth] date NOT NULL,
        [SocNumber] nvarchar(max) NOT NULL,
        [Sex] nvarchar(max) NOT NULL,
        [Graduated] bit NOT NULL,
        [EduYear] nvarchar(max) NOT NULL,
        [GroupId] nvarchar(max) NOT NULL,
        [ClassroomGrade] int NOT NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_NmuhStudents] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [NmuhStudentsStaging] (
        [Id] uniqueidentifier NOT NULL,
        [NmuhStudentId] int NOT NULL,
        [NmuhSchoolId] int NOT NULL,
        [InternalSchoolId] uniqueidentifier NULL,
        [RegionId] int NOT NULL,
        [SchoolName] nvarchar(max) NOT NULL,
        [Marz] nvarchar(max) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [DateOfBirth] date NOT NULL,
        [SocNumber] nvarchar(max) NOT NULL,
        [Sex] nvarchar(max) NOT NULL,
        [Graduated] bit NOT NULL,
        [EduYear] nvarchar(max) NOT NULL,
        [GroupId] nvarchar(max) NOT NULL,
        [ClassroomGrade] int NOT NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_NmuhStudentsStaging] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [PupilsStaging] (
        [Id] uniqueidentifier NOT NULL,
        [KtakPupilId] int NOT NULL,
        [KtakSchoolId] int NOT NULL,
        [RegionId] int NOT NULL,
        [ClassroomId] nvarchar(max) NOT NULL,
        [ClassroomInternalId] uniqueidentifier NULL,
        [Place] int NOT NULL,
        [Grade] int NOT NULL,
        [SubGrade] int NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [SocNumber] nvarchar(max) NOT NULL,
        [CertificateType] int NOT NULL,
        [Certificate] nvarchar(max) NOT NULL,
        [Birthday] date NOT NULL,
        [Gender] bit NOT NULL,
        [Status] int NOT NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PupilsStaging] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [SchoolEmployees] (
        [Id] uniqueidentifier NOT NULL,
        [PersonId] int NOT NULL,
        [SchoolId] int NOT NULL,
        [RegionId] int NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [Sex] nvarchar(max) NOT NULL,
        [SocNumber] nvarchar(max) NOT NULL,
        [DateOfBirth] date NULL,
        [Address] nvarchar(max) NOT NULL,
        [Phone] nvarchar(max) NOT NULL,
        [MainSubjectId] nvarchar(max) NULL,
        [Position] nvarchar(max) NOT NULL,
        [StaffGroup] nvarchar(max) NOT NULL,
        [VacationId] int NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SchoolEmployees] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [Teachers] (
        [Id] uniqueidentifier NOT NULL,
        [KtakTeacherId] int NOT NULL,
        [KtakSchoolId] int NOT NULL,
        [RegionId] int NOT NULL,
        [Place] int NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [Gender] bit NOT NULL,
        [Birthday] date NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [SocNumber] nvarchar(max) NOT NULL,
        [Experience] int NOT NULL,
        [AcademicRank] int NOT NULL,
        [Education] int NOT NULL,
        [CommandDate] datetime2 NULL,
        [DigitLevel] int NOT NULL,
        [Activated] nvarchar(max) NOT NULL,
        [WorkType] nvarchar(max) NOT NULL,
        [MainSubjectId] nvarchar(max) NOT NULL,
        [MainSubject] nvarchar(max) NOT NULL,
        [PersonPositions] nvarchar(max) NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Teachers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [TeachersStaging] (
        [Id] uniqueidentifier NOT NULL,
        [KtakTeacherId] int NOT NULL,
        [KtakSchoolId] int NOT NULL,
        [RegionId] int NOT NULL,
        [Place] int NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [Gender] bit NOT NULL,
        [Birthday] date NULL,
        [Phone] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [SocNumber] nvarchar(max) NOT NULL,
        [Experience] int NOT NULL,
        [AcademicRank] int NOT NULL,
        [Education] int NOT NULL,
        [CommandDate] datetime2 NULL,
        [DigitLevel] int NOT NULL,
        [Activated] nvarchar(max) NOT NULL,
        [WorkType] nvarchar(max) NOT NULL,
        [MainSubjectId] nvarchar(max) NOT NULL,
        [MainSubject] nvarchar(max) NOT NULL,
        [PersonPositions] nvarchar(max) NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_TeachersStaging] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [Schools] (
        [DshhSchoolId] uniqueidentifier NOT NULL,
        [KtakSchoolId] int NOT NULL,
        [RegionId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Marz] nvarchar(max) NOT NULL,
        [Region] nvarchar(max) NOT NULL,
        [Community] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [EmployeeId] int NULL,
        CONSTRAINT [PK_Schools] PRIMARY KEY ([DshhSchoolId]),
        CONSTRAINT [FK_Schools_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [MmuhStaffGroups] (
        [Id] uniqueidentifier NOT NULL,
        [MmuhStaffId] uniqueidentifier NOT NULL,
        [GroupId] int NOT NULL,
        [GroupName] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_MmuhStaffGroups] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MmuhStaffGroups_MmuhStaff_MmuhStaffId] FOREIGN KEY ([MmuhStaffId]) REFERENCES [MmuhStaff] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [NmuhStaffGroups] (
        [Id] uniqueidentifier NOT NULL,
        [NmuhStaffId] uniqueidentifier NOT NULL,
        [GroupId] int NOT NULL,
        [GroupName] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_NmuhStaffGroups] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NmuhStaffGroups_NmuhStaff_NmuhStaffId] FOREIGN KEY ([NmuhStaffId]) REFERENCES [NmuhStaff] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [TeacherSubjects] (
        [Id] uniqueidentifier NOT NULL,
        [TeacherId] uniqueidentifier NOT NULL,
        [SubjectId] int NOT NULL,
        [Grade] int NOT NULL,
        [SubGrade] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_TeacherSubjects] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TeacherSubjects_Teachers_TeacherId] FOREIGN KEY ([TeacherId]) REFERENCES [Teachers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [Classrooms] (
        [Id] uniqueidentifier NOT NULL,
        [KtakSchoolId] int NOT NULL,
        [KtakClassroomId] nvarchar(450) NOT NULL,
        [RegionId] int NOT NULL,
        [Grade] nvarchar(max) NOT NULL,
        [Classifier] nvarchar(max) NOT NULL,
        [ClassName] nvarchar(max) NOT NULL,
        [Stream] nvarchar(max) NULL,
        [SchoolId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Classrooms] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Classrooms_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([DshhSchoolId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [MmuhSubjects] (
        [Id] uniqueidentifier NOT NULL,
        [MmuhStaffGroupId] uniqueidentifier NOT NULL,
        [SubjectId] int NOT NULL,
        [SubjectName] nvarchar(max) NOT NULL,
        [SubjectType] nvarchar(max) NOT NULL,
        [SubjectTypeId] int NOT NULL,
        CONSTRAINT [PK_MmuhSubjects] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MmuhSubjects_MmuhStaffGroups_MmuhStaffGroupId] FOREIGN KEY ([MmuhStaffGroupId]) REFERENCES [MmuhStaffGroups] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [NmuhSubjects] (
        [Id] uniqueidentifier NOT NULL,
        [NmuhStaffGroupId] uniqueidentifier NOT NULL,
        [SubjectId] int NOT NULL,
        [SubjectName] nvarchar(max) NOT NULL,
        [SubjectType] nvarchar(max) NOT NULL,
        [SubjectTypeId] int NOT NULL,
        CONSTRAINT [PK_NmuhSubjects] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NmuhSubjects_NmuhStaffGroups_NmuhStaffGroupId] FOREIGN KEY ([NmuhStaffGroupId]) REFERENCES [NmuhStaffGroups] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE TABLE [Pupils] (
        [Id] uniqueidentifier NOT NULL,
        [KtakPupilId] int NOT NULL,
        [KtakSchoolId] int NOT NULL,
        [RegionId] int NOT NULL,
        [ClassroomId] nvarchar(450) NOT NULL,
        [ClassroomInternalId] uniqueidentifier NULL,
        [Place] int NOT NULL,
        [Grade] int NOT NULL,
        [SubGrade] int NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [FatherName] nvarchar(max) NOT NULL,
        [SocNumber] nvarchar(max) NOT NULL,
        [CertificateType] int NOT NULL,
        [Certificate] nvarchar(max) NOT NULL,
        [Birthday] date NOT NULL,
        [Gender] bit NOT NULL,
        [Status] int NOT NULL,
        [MD5] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Pupils] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Pupils_Classrooms_ClassroomInternalId] FOREIGN KEY ([ClassroomInternalId]) REFERENCES [Classrooms] ([Id]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Classrooms_KtakSchoolId_KtakClassroomId] ON [Classrooms] ([KtakSchoolId], [KtakClassroomId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Classrooms_SchoolId] ON [Classrooms] ([SchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MmuhInstitutions_InstId] ON [MmuhInstitutions] ([InstId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MmuhInstitutions_RegionId] ON [MmuhInstitutions] ([RegionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MmuhStaff_InstId] ON [MmuhStaff] ([InstId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MmuhStaff_MmuhStaffId] ON [MmuhStaff] ([MmuhStaffId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MmuhStaffGroups_MmuhStaffId] ON [MmuhStaffGroups] ([MmuhStaffId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MmuhStudents_MmuhSchoolId] ON [MmuhStudents] ([MmuhSchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MmuhStudents_MmuhStudentId] ON [MmuhStudents] ([MmuhStudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MmuhSubjects_MmuhStaffGroupId] ON [MmuhSubjects] ([MmuhStaffGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NmuhInstitutions_InstId] ON [NmuhInstitutions] ([InstId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NmuhInstitutions_RegionId] ON [NmuhInstitutions] ([RegionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NmuhStaff_InstId] ON [NmuhStaff] ([InstId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NmuhStaff_NmuhStaffId] ON [NmuhStaff] ([NmuhStaffId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NmuhStaffGroups_NmuhStaffId] ON [NmuhStaffGroups] ([NmuhStaffId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NmuhStudents_NmuhSchoolId] ON [NmuhStudents] ([NmuhSchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NmuhStudents_NmuhStudentId] ON [NmuhStudents] ([NmuhStudentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_NmuhSubjects_NmuhStaffGroupId] ON [NmuhSubjects] ([NmuhStaffGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Pupils_ClassroomInternalId] ON [Pupils] ([ClassroomInternalId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Pupils_KtakPupilId] ON [Pupils] ([KtakPupilId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Pupils_KtakSchoolId_ClassroomId_Place] ON [Pupils] ([KtakSchoolId], [ClassroomId], [Place]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SchoolEmployees_PersonId] ON [SchoolEmployees] ([PersonId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SchoolEmployees_RegionId] ON [SchoolEmployees] ([RegionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SchoolEmployees_SchoolId] ON [SchoolEmployees] ([SchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Schools_EmployeeId] ON [Schools] ([EmployeeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Schools_KtakSchoolId] ON [Schools] ([KtakSchoolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Schools_RegionId] ON [Schools] ([RegionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Teachers_KtakTeacherId] ON [Teachers] ([KtakTeacherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TeacherSubjects_TeacherId] ON [TeacherSubjects] ([TeacherId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260325072649_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260325072649_InitialCreate', N'9.0.11');
END;

COMMIT;
GO

