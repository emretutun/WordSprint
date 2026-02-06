using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordSprint.Infrastructure.Persistence;

namespace WordSprint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly WordSprintDbContext _db;

    public LeaderboardController(WordSprintDbContext db)
    {
        _db = db;
    }

    // ✅ 1) Toplam learned kelime liderliği (Top 10)
    // /api/leaderboard/top-learned?limit=10
    [HttpGet("top-learned")]
    public async Task<IActionResult> TopLearned([FromQuery] int limit = 10)
    {
        limit = Math.Clamp(limit, 1, 50);

        var rows = await _db.UserWords
            .Where(x => x.IsLearned)
            .GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, LearnedCount = g.Count() })
            .OrderByDescending(x => x.LearnedCount)
            .Take(limit)
            .Join(
                _db.Users,
                a => a.UserId,
                u => u.Id,
                (a, u) => new
                {
                    userId = u.Id,
                    name = ((u.FirstName ?? "") + " " + (u.LastName ?? "")).Trim(),
                    email = u.Email,
                    learnedCount = a.LearnedCount
                }
            )
            .ToListAsync();

        return Ok(rows);
    }

    // ✅ 2) Gün sayısı liderliği (Top 5) -> “kaç gündür öğreniyor”
    // /api/leaderboard/top-days?limit=5
    [HttpGet("top-days")]
    public async Task<IActionResult> TopDays([FromQuery] int limit = 5)
    {
        limit = Math.Clamp(limit, 1, 50);

        var rows = await _db.UserDailyActivities
            .Where(x => x.LearnedCount > 0 || x.QuizCount > 0)
            .GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Days = g.Count() })
            .OrderByDescending(x => x.Days)
            .Take(limit)
            .Join(
                _db.Users,
                a => a.UserId,
                u => u.Id,
                (a, u) => new
                {
                    userId = u.Id,
                    name = ((u.FirstName ?? "") + " " + (u.LastName ?? "")).Trim(),
                    email = u.Email,
                    daysLearning = a.Days
                }
            )
            .ToListAsync();

        return Ok(rows);
    }
}
