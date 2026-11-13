using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PordznakanAPI.Migrations
{
    /// <inheritdoc />
    public partial class UniquePupilKtakId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pupils_KtakPupilId",
                table: "Pupils");

            migrationBuilder.CreateIndex(
                name: "IX_Pupils_KtakPupilId",
                table: "Pupils",
                column: "KtakPupilId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pupils_KtakPupilId",
                table: "Pupils");

            migrationBuilder.CreateIndex(
                name: "IX_Pupils_KtakPupilId",
                table: "Pupils",
                column: "KtakPupilId");
        }
    }
}
