using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PordznakanAPI.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateMuhStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MuhStudents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MuhStudentId = table.Column<int>(type: "int", nullable: false),
                    InstitutionId = table.Column<int>(type: "int", nullable: false),
                    InternalSchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    InstitutionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentDocument = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentDocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromCountry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FreeEdu = table.Column<bool>(type: "bit", nullable: false),
                    HasBook = table.Column<bool>(type: "bit", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuhStudents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MuhStudentsStaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MuhStudentId = table.Column<int>(type: "int", nullable: false),
                    InstitutionId = table.Column<int>(type: "int", nullable: false),
                    InternalSchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    InstitutionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentDocument = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentDocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromCountry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FreeEdu = table.Column<bool>(type: "bit", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuhStudentsStaging", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MuhStudents_InstitutionId",
                table: "MuhStudents",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_MuhStudents_MuhStudentId",
                table: "MuhStudents",
                column: "MuhStudentId");

            // The old tables may already be gone (e.g. dropped by hand); then there is nothing to copy.
            // Carry the existing students (and, above all, their HasBook flag) over to the new table
            // before the old ones are dropped. The columns the old tables did not have are left empty and
            // MD5 is '' so the next MuhStudent sync fills them in. MMUH rows win when an id exists in both.
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[MmuhStudents]', N'U') IS NOT NULL
INSERT INTO [MuhStudents]
    ([Id], [MuhStudentId], [InstitutionId], [InternalSchoolId], [RegionId], [InstitutionName], [GroupId], [GroupName],
     [FirstName], [LastName], [FatherName], [IdentDocument], [IdentDocumentNumber], [FromCountry], [SocNumber],
     [DateOfBirth], [Sex], [FreeEdu], [HasBook], [MD5], [CreatedAt], [UpdatedAt])
SELECT [Id], [MmuhStudentId], [MmuhSchoolId], [InternalSchoolId], [RegionId], [SchoolName], ISNULL(TRY_CAST([GroupId] AS int), 0), N'',
       [FirstName], [LastName], [FatherName], N'', N'', N'', [SocNumber],
       [DateOfBirth], [Sex], 0, [HasBook], N'', [CreatedAt], [UpdatedAt]
FROM [MmuhStudents];

IF OBJECT_ID(N'[NmuhStudents]', N'U') IS NOT NULL
INSERT INTO [MuhStudents]
    ([Id], [MuhStudentId], [InstitutionId], [InternalSchoolId], [RegionId], [InstitutionName], [GroupId], [GroupName],
     [FirstName], [LastName], [FatherName], [IdentDocument], [IdentDocumentNumber], [FromCountry], [SocNumber],
     [DateOfBirth], [Sex], [FreeEdu], [HasBook], [MD5], [CreatedAt], [UpdatedAt])
SELECT n.[Id], n.[NmuhStudentId], n.[NmuhSchoolId], n.[InternalSchoolId], n.[RegionId], n.[SchoolName], ISNULL(TRY_CAST(n.[GroupId] AS int), 0), N'',
       n.[FirstName], n.[LastName], n.[FatherName], N'', N'', N'', n.[SocNumber],
       n.[DateOfBirth], n.[Sex], 0, n.[HasBook], N'', n.[CreatedAt], n.[UpdatedAt]
FROM [NmuhStudents] n
WHERE NOT EXISTS (SELECT 1 FROM [MuhStudents] m WHERE m.[MuhStudentId] = n.[NmuhStudentId]);
");

            migrationBuilder.Sql("DROP TABLE IF EXISTS [MmuhStudents];");

            migrationBuilder.Sql("DROP TABLE IF EXISTS [MmuhStudentsStaging];");

            migrationBuilder.Sql("DROP TABLE IF EXISTS [NmuhStudents];");

            migrationBuilder.Sql("DROP TABLE IF EXISTS [NmuhStudentsStaging];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MuhStudents");

            migrationBuilder.DropTable(
                name: "MuhStudentsStaging");

            migrationBuilder.CreateTable(
                name: "MmuhStudents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassroomGrade = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Graduated = table.Column<bool>(type: "bit", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasBook = table.Column<bool>(type: "bit", nullable: false),
                    InternalSchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MmuhSchoolId = table.Column<int>(type: "int", nullable: false),
                    MmuhStudentId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MmuhStudents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MmuhStudentsStaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassroomGrade = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Graduated = table.Column<bool>(type: "bit", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InternalSchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MmuhSchoolId = table.Column<int>(type: "int", nullable: false),
                    MmuhStudentId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MmuhStudentsStaging", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NmuhStudents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassroomGrade = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    EduYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Graduated = table.Column<bool>(type: "bit", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasBook = table.Column<bool>(type: "bit", nullable: false),
                    InternalSchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NmuhSchoolId = table.Column<int>(type: "int", nullable: false),
                    NmuhStudentId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NmuhStudents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NmuhStudentsStaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassroomGrade = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    EduYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Graduated = table.Column<bool>(type: "bit", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InternalSchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NmuhSchoolId = table.Column<int>(type: "int", nullable: false),
                    NmuhStudentId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NmuhStudentsStaging", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MmuhStudents_MmuhSchoolId",
                table: "MmuhStudents",
                column: "MmuhSchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_MmuhStudents_MmuhStudentId",
                table: "MmuhStudents",
                column: "MmuhStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_NmuhStudents_NmuhSchoolId",
                table: "NmuhStudents",
                column: "NmuhSchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_NmuhStudents_NmuhStudentId",
                table: "NmuhStudents",
                column: "NmuhStudentId");
        }
    }
}
