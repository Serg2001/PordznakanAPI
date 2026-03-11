using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PordznakanAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalPersonId = table.Column<int>(type: "int", nullable: false),
                    Ssn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MmuhStaff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MmuhStaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InstId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    InstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Citizenship = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentDocument = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentDocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromCountry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InFiz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Druyq = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartlyIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartlyInstNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionDetailId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionDetailName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MmuhStaff", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MmuhStaffStaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MmuhStaffId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    InstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Citizenship = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentDocument = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentDocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromCountry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InFiz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Druyq = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartlyIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartlyInstNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionDetailId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionDetailName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MmuhStaffStaging", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MmuhStudents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MmuhStudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MmuhSchoolId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Graduated = table.Column<bool>(type: "bit", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassroomGrade = table.Column<int>(type: "int", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    MmuhStudentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MmuhSchoolId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Graduated = table.Column<bool>(type: "bit", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassroomGrade = table.Column<int>(type: "int", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MmuhStudentsStaging", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NmuhStaff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NmuhStaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InstId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    InstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Citizenship = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentDocument = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentDocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromCountry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InFiz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Druyq = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartlyIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartlyInstNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionDetailId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionDetailName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NmuhStaff", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NmuhStaffStaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NmuhStaffId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    InstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Citizenship = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentDocument = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentDocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromCountry = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InFiz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Druyq = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartlyIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartlyInstNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PositionName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionDetailId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionDetailName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NmuhStaffStaging", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NmuhStudents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NmuhStudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NmuhSchoolId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Graduated = table.Column<bool>(type: "bit", nullable: false),
                    EduYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassroomGrade = table.Column<int>(type: "int", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    NmuhStudentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NmuhSchoolId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Graduated = table.Column<bool>(type: "bit", nullable: false),
                    EduYear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassroomGrade = table.Column<int>(type: "int", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NmuhStudentsStaging", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PupilsStaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KtakPupilId = table.Column<int>(type: "int", nullable: false),
                    KtakSchoolId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    ClassroomId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassroomInternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Place = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    SubGrade = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CertificateType = table.Column<int>(type: "int", nullable: false),
                    Certificate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Birthday = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PupilsStaging", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KtakTeacherId = table.Column<int>(type: "int", nullable: false),
                    KtakSchoolId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    Place = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<bool>(type: "bit", nullable: false),
                    Birthday = table.Column<DateOnly>(type: "date", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    AcademicRank = table.Column<int>(type: "int", nullable: false),
                    Education = table.Column<int>(type: "int", nullable: false),
                    CommandDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DigitLevel = table.Column<int>(type: "int", nullable: false),
                    Activated = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MainSubjectId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MainSubject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonPositions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeachersStaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KtakTeacherId = table.Column<int>(type: "int", nullable: false),
                    KtakSchoolId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    Place = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<bool>(type: "bit", nullable: false),
                    Birthday = table.Column<DateOnly>(type: "date", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    AcademicRank = table.Column<int>(type: "int", nullable: false),
                    Education = table.Column<int>(type: "int", nullable: false),
                    CommandDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DigitLevel = table.Column<int>(type: "int", nullable: false),
                    Activated = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MainSubjectId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MainSubject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonPositions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachersStaging", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    DshhSchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KtakSchoolId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marz = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Community = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.DshhSchoolId);
                    table.ForeignKey(
                        name: "FK_Schools_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeacherSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    SubGrade = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherSubjects_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Classrooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KtakSchoolId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    KtakClassroomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Classifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stream = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classrooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classrooms_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "DshhSchoolId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pupils",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KtakPupilId = table.Column<int>(type: "int", nullable: false),
                    KtakSchoolId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    ClassroomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClassroomInternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Place = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    SubGrade = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CertificateType = table.Column<int>(type: "int", nullable: false),
                    Certificate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Birthday = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pupils", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pupils_Classrooms_ClassroomInternalId",
                        column: x => x.ClassroomInternalId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_KtakSchoolId_KtakClassroomId",
                table: "Classrooms",
                columns: new[] { "KtakSchoolId", "KtakClassroomId" });

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_SchoolId",
                table: "Classrooms",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_MmuhStaff_InstId",
                table: "MmuhStaff",
                column: "InstId");

            migrationBuilder.CreateIndex(
                name: "IX_MmuhStaff_MmuhStaffId",
                table: "MmuhStaff",
                column: "MmuhStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MmuhStudents_MmuhSchoolId",
                table: "MmuhStudents",
                column: "MmuhSchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_MmuhStudents_MmuhStudentId",
                table: "MmuhStudents",
                column: "MmuhStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_NmuhStaff_InstId",
                table: "NmuhStaff",
                column: "InstId");

            migrationBuilder.CreateIndex(
                name: "IX_NmuhStaff_NmuhStaffId",
                table: "NmuhStaff",
                column: "NmuhStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_NmuhStudents_NmuhSchoolId",
                table: "NmuhStudents",
                column: "NmuhSchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_NmuhStudents_NmuhStudentId",
                table: "NmuhStudents",
                column: "NmuhStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Pupils_ClassroomInternalId",
                table: "Pupils",
                column: "ClassroomInternalId");

            migrationBuilder.CreateIndex(
                name: "IX_Pupils_KtakPupilId",
                table: "Pupils",
                column: "KtakPupilId");

            migrationBuilder.CreateIndex(
                name: "IX_Pupils_KtakSchoolId_ClassroomId_Place",
                table: "Pupils",
                columns: new[] { "KtakSchoolId", "ClassroomId", "Place" });

            migrationBuilder.CreateIndex(
                name: "IX_Schools_EmployeeId",
                table: "Schools",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_KtakSchoolId",
                table: "Schools",
                column: "KtakSchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_RegionId",
                table: "Schools",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_KtakTeacherId",
                table: "Teachers",
                column: "KtakTeacherId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjects_TeacherId",
                table: "TeacherSubjects",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MmuhStaff");

            migrationBuilder.DropTable(
                name: "MmuhStaffStaging");

            migrationBuilder.DropTable(
                name: "MmuhStudents");

            migrationBuilder.DropTable(
                name: "MmuhStudentsStaging");

            migrationBuilder.DropTable(
                name: "NmuhStaff");

            migrationBuilder.DropTable(
                name: "NmuhStaffStaging");

            migrationBuilder.DropTable(
                name: "NmuhStudents");

            migrationBuilder.DropTable(
                name: "NmuhStudentsStaging");

            migrationBuilder.DropTable(
                name: "Pupils");

            migrationBuilder.DropTable(
                name: "PupilsStaging");

            migrationBuilder.DropTable(
                name: "TeachersStaging");

            migrationBuilder.DropTable(
                name: "TeacherSubjects");

            migrationBuilder.DropTable(
                name: "Classrooms");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
