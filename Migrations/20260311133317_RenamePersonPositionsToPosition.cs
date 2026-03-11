using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PordznakanAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenamePersonPositionsToPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PersonPositions",
                table: "SchoolEmployees",
                newName: "Position");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Position",
                table: "SchoolEmployees",
                newName: "PersonPositions");
        }
    }
}
