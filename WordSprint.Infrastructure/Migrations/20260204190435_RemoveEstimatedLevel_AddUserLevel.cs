using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSprint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEstimatedLevel_AddUserLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedLevel",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstimatedLevel",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }
    }
}
