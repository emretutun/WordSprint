using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSprint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDailyActivityLeaderboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDailyActivity",
                table: "UserDailyActivity");

            migrationBuilder.RenameTable(
                name: "UserDailyActivity",
                newName: "UserDailyActivities");

            migrationBuilder.RenameIndex(
                name: "IX_UserDailyActivity_UserId_DayUtc",
                table: "UserDailyActivities",
                newName: "IX_UserDailyActivities_UserId_DayUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDailyActivities",
                table: "UserDailyActivities",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDailyActivities",
                table: "UserDailyActivities");

            migrationBuilder.RenameTable(
                name: "UserDailyActivities",
                newName: "UserDailyActivity");

            migrationBuilder.RenameIndex(
                name: "IX_UserDailyActivities_UserId_DayUtc",
                table: "UserDailyActivity",
                newName: "IX_UserDailyActivity_UserId_DayUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDailyActivity",
                table: "UserDailyActivity",
                column: "Id");
        }
    }
}
