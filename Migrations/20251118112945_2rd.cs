using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PordznakanAPI.Migrations
{
    /// <inheritdoc />
    public partial class _2rd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_Employees_DirectorId",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Schools_DirectorId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Community",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "DirectorId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Marz",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Schools");

            migrationBuilder.AlterColumn<int>(
                name: "KtakId",
                table: "Schools",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "Schools",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KtakId",
                table: "Classrooms",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_EmployeeId",
                table: "Schools",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_KtakId",
                table: "Schools",
                column: "KtakId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_KtakId",
                table: "Classrooms",
                column: "KtakId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_Employees_EmployeeId",
                table: "Schools",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_Employees_EmployeeId",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Schools_EmployeeId",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Schools_KtakId",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Classrooms_KtakId",
                table: "Classrooms");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Schools");

            migrationBuilder.AlterColumn<string>(
                name: "KtakId",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Community",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DirectorId",
                table: "Schools",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Marz",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Schools",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "KtakId",
                table: "Classrooms",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_DirectorId",
                table: "Schools",
                column: "DirectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_Employees_DirectorId",
                table: "Schools",
                column: "DirectorId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
