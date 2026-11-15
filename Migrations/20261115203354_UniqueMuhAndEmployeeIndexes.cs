using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PordznakanAPI.Migrations
{
    /// <inheritdoc />
    public partial class UniqueMuhAndEmployeeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MuhStudents_MuhStudentId",
                table: "MuhStudents");

            migrationBuilder.DropIndex(
                name: "IX_MuhInstitutions_InstitutionId",
                table: "MuhInstitutions");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolEmployees_RegionId_PersonId_VacationId",
                table: "SchoolEmployees",
                columns: new[] { "RegionId", "PersonId", "VacationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MuhStudents_MuhStudentId",
                table: "MuhStudents",
                column: "MuhStudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MuhInstitutions_InstitutionId",
                table: "MuhInstitutions",
                column: "InstitutionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchoolEmployees_RegionId_PersonId_VacationId",
                table: "SchoolEmployees");

            migrationBuilder.DropIndex(
                name: "IX_MuhStudents_MuhStudentId",
                table: "MuhStudents");

            migrationBuilder.DropIndex(
                name: "IX_MuhInstitutions_InstitutionId",
                table: "MuhInstitutions");

            migrationBuilder.CreateIndex(
                name: "IX_MuhStudents_MuhStudentId",
                table: "MuhStudents",
                column: "MuhStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MuhInstitutions_InstitutionId",
                table: "MuhInstitutions",
                column: "InstitutionId");
        }
    }
}
