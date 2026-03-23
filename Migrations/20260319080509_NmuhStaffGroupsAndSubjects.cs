using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PordznakanAPI.Migrations
{
    /// <inheritdoc />
    public partial class NmuhStaffGroupsAndSubjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "NmuhStaff");

            migrationBuilder.RenameColumn(
                name: "GroupsJson",
                table: "NmuhStaff",
                newName: "GroupIds");

            migrationBuilder.AlterColumn<bool>(
                name: "Sex",
                table: "NmuhStaffStaging",
                type: "bit",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "Sex",
                table: "NmuhStaff",
                type: "bit",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "NmuhStaffGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NmuhStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NmuhStaffGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NmuhStaffGroups_NmuhStaff_NmuhStaffId",
                        column: x => x.NmuhStaffId,
                        principalTable: "NmuhStaff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NmuhSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NmuhStaffGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubjectType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubjectTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NmuhSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NmuhSubjects_NmuhStaffGroups_NmuhStaffGroupId",
                        column: x => x.NmuhStaffGroupId,
                        principalTable: "NmuhStaffGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NmuhStaffGroups_NmuhStaffId",
                table: "NmuhStaffGroups",
                column: "NmuhStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_NmuhSubjects_NmuhStaffGroupId",
                table: "NmuhSubjects",
                column: "NmuhStaffGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NmuhSubjects");

            migrationBuilder.DropTable(
                name: "NmuhStaffGroups");

            migrationBuilder.RenameColumn(
                name: "GroupIds",
                table: "NmuhStaff",
                newName: "GroupsJson");

            migrationBuilder.AlterColumn<string>(
                name: "Sex",
                table: "NmuhStaffStaging",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Sex",
                table: "NmuhStaff",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<string>(
                name: "GroupId",
                table: "NmuhStaff",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
