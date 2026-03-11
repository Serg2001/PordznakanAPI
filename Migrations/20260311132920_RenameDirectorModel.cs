using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PordznakanAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameDirectorModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Directors");

            migrationBuilder.CreateTable(
                name: "SchoolEmployees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MainSubjectId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonPositions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolEmployees", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolEmployees_PersonId",
                table: "SchoolEmployees",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolEmployees_RegionId",
                table: "SchoolEmployees",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolEmployees_SchoolId",
                table: "SchoolEmployees",
                column: "SchoolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolEmployees");

            migrationBuilder.CreateTable(
                name: "Directors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MD5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    SocNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Directors_PersonId",
                table: "Directors",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Directors_RegionId",
                table: "Directors",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Directors_SchoolId",
                table: "Directors",
                column: "SchoolId");
        }
    }
}
