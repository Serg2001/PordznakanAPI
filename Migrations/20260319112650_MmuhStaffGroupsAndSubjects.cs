using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PordznakanAPI.Migrations
{
    /// <inheritdoc />
    public partial class MmuhStaffGroupsAndSubjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "MmuhStaff");

            migrationBuilder.RenameColumn(
                name: "GroupsJson",
                table: "MmuhStaff",
                newName: "GroupIds");

            migrationBuilder.AlterColumn<bool>(
                name: "Sex",
                table: "MmuhStaffStaging",
                type: "bit",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "SexRaw",
                table: "MmuhStaffStaging",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "Sex",
                table: "MmuhStaff",
                type: "bit",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "MmuhStaffGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MmuhStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MmuhStaffGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MmuhStaffGroups_MmuhStaff_MmuhStaffId",
                        column: x => x.MmuhStaffId,
                        principalTable: "MmuhStaff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MmuhSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MmuhStaffGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubjectType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubjectTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MmuhSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MmuhSubjects_MmuhStaffGroups_MmuhStaffGroupId",
                        column: x => x.MmuhStaffGroupId,
                        principalTable: "MmuhStaffGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MmuhStaffGroups_MmuhStaffId",
                table: "MmuhStaffGroups",
                column: "MmuhStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MmuhSubjects_MmuhStaffGroupId",
                table: "MmuhSubjects",
                column: "MmuhStaffGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MmuhSubjects");

            migrationBuilder.DropTable(
                name: "MmuhStaffGroups");

            migrationBuilder.DropColumn(
                name: "SexRaw",
                table: "MmuhStaffStaging");

            migrationBuilder.RenameColumn(
                name: "GroupIds",
                table: "MmuhStaff",
                newName: "GroupsJson");

            migrationBuilder.AlterColumn<string>(
                name: "Sex",
                table: "MmuhStaffStaging",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Sex",
                table: "MmuhStaff",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<string>(
                name: "GroupId",
                table: "MmuhStaff",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
