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
                name: "PupilsStaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KtakPupilId = table.Column<int>(type: "int", nullable: false),
                    KtakSchoolId = table.Column<int>(type: "int", nullable: false),
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
                    PersonId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SchoolId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activated = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Education = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommandDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubjectId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MainSubject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonPositions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubjectsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DigitLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Experience = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcademicRank = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcademicRankId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
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
                name: "Classrooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KtakSchoolId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    KtakClassroomId = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                name: "IX_Teachers_PersonId",
                table: "Teachers",
                column: "PersonId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pupils");

            migrationBuilder.DropTable(
                name: "PupilsStaging");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "Classrooms");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
