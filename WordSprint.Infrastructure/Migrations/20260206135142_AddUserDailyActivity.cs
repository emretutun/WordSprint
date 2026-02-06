using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WordSprint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDailyActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserDailyActivity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    DayUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LearnedCount = table.Column<int>(type: "integer", nullable: false),
                    QuizCount = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyActivity", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyActivity_UserId_DayUtc",
                table: "UserDailyActivity",
                columns: new[] { "UserId", "DayUtc" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDailyActivity");
        }
    }
}
