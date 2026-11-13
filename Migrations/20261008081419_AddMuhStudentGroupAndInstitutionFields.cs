using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PordznakanAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddMuhStudentGroupAndInstitutionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "MuhStudentsStaging",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualFee",
                table: "MuhStudentsStaging",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Base",
                table: "MuhStudentsStaging",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Community",
                table: "MuhStudentsStaging",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "MuhStudentsStaging",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EduProgramCode",
                table: "MuhStudentsStaging",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EduProgramProfCode",
                table: "MuhStudentsStaging",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EduProgramProfTitle",
                table: "MuhStudentsStaging",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EduProgramTitle",
                table: "MuhStudentsStaging",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EduYear",
                table: "MuhStudentsStaging",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GroupType",
                table: "MuhStudentsStaging",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InstitutionEmail",
                table: "MuhStudentsStaging",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "InstitutionType",
                table: "MuhStudentsStaging",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InstitutionTypeName",
                table: "MuhStudentsStaging",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Marz",
                table: "MuhStudentsStaging",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "MuhStudentsStaging",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VeGroupTypes",
                table: "MuhStudentsStaging",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "MuhStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualFee",
                table: "MuhStudents",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Base",
                table: "MuhStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Community",
                table: "MuhStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "MuhStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EduProgramCode",
                table: "MuhStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EduProgramProfCode",
                table: "MuhStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EduProgramProfTitle",
                table: "MuhStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EduProgramTitle",
                table: "MuhStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EduYear",
                table: "MuhStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GroupType",
                table: "MuhStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InstitutionEmail",
                table: "MuhStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "InstitutionType",
                table: "MuhStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InstitutionTypeName",
                table: "MuhStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Marz",
                table: "MuhStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "MuhStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VeGroupTypes",
                table: "MuhStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "AnnualFee",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "Base",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "Community",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "EduProgramCode",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "EduProgramProfCode",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "EduProgramProfTitle",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "EduProgramTitle",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "EduYear",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "GroupType",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "InstitutionEmail",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "InstitutionType",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "InstitutionTypeName",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "Marz",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "VeGroupTypes",
                table: "MuhStudentsStaging");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "AnnualFee",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "Base",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "Community",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "EduProgramCode",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "EduProgramProfCode",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "EduProgramProfTitle",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "EduProgramTitle",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "EduYear",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "GroupType",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "InstitutionEmail",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "InstitutionType",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "InstitutionTypeName",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "Marz",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "MuhStudents");

            migrationBuilder.DropColumn(
                name: "VeGroupTypes",
                table: "MuhStudents");
        }
    }
}
