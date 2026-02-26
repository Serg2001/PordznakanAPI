using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PordznakanAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLogEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogEmployees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LogId = table.Column<int>(type: "int", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Received = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Transferred = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogEmployees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MmuhStaff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MmuhStaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InstId = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                name: "NmuhStaff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NmuhStaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InstId = table.Column<string>(type: "nvarchar(450)", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_LogEmployees_LogId",
                table: "LogEmployees",
                column: "LogId");

            migrationBuilder.CreateIndex(
                name: "IX_LogEmployees_SchoolId",
                table: "LogEmployees",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogEmployees");

            migrationBuilder.DropTable(
                name: "MmuhStaff");

            migrationBuilder.DropTable(
                name: "MmuhStaffStaging");

            migrationBuilder.DropTable(
                name: "NmuhStaff");

            migrationBuilder.DropTable(
                name: "NmuhStaffStaging");

            migrationBuilder.DropTable(
                name: "NmuhStudents");

            migrationBuilder.DropTable(
                name: "NmuhStudentsStaging");
        }
    }
}
