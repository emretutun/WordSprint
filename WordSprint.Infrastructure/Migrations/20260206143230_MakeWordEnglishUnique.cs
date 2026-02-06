using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSprint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeWordEnglishUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Words_English",
                table: "Words");

            migrationBuilder.CreateIndex(
                name: "IX_Words_English",
                table: "Words",
                column: "English",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Words_English",
                table: "Words");

            migrationBuilder.CreateIndex(
                name: "IX_Words_English",
                table: "Words",
                column: "English");
        }
    }
}
