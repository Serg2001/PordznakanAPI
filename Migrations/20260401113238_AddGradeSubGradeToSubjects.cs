using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PordznakanAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeSubGradeToSubjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "NmuhSubjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubGrade",
                table: "NmuhSubjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "MmuhSubjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubGrade",
                table: "MmuhSubjects",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grade",
                table: "NmuhSubjects");

            migrationBuilder.DropColumn(
                name: "SubGrade",
                table: "NmuhSubjects");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "MmuhSubjects");

            migrationBuilder.DropColumn(
                name: "SubGrade",
                table: "MmuhSubjects");
        }
    }
}
