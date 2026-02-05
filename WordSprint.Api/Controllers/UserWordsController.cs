using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WordSprint.Core.Entities;
using WordSprint.Infrastructure.Persistence;
using WordSprint.Core.Enums;
using WordSprint.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace WordSprint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserWordsController : ControllerBase
{
    private readonly WordSprintDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserWordsController(WordSprintDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // Kullanıcıya yeni 10 kelime atar (zaten atanmışsa tekrar eklemez)
    [HttpPost("assign-random")]
    public async Task<IActionResult> AssignRandom([FromQuery] int count = 10)
    {
        if (count <= 0 || count > 50)
            return BadRequest("count must be between 1 and 50.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        // kullanıcı level’ını al
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        short u = (short)user.Level;
        short min = (short)Math.Max(u - 1, 0);

        // kullanıcının zaten sahip olduğu wordId’ler
        var ownedIds = _db.UserWords
            .Where(uw => uw.UserId == userId)
            .Select(uw => uw.WordId);

        // A2 => A1+A2 filtresi burada
        var wordIdsToAssign = await _db.Words
            .Where(w => (short)w.Level >= min && (short)w.Level <= u)
            .Where(w => !ownedIds.Contains(w.Id))
            .OrderBy(_ => Guid.NewGuid())
            .Take(count)
            .Select(w => w.Id)
            .ToListAsync();

        if (wordIdsToAssign.Count == 0)
            return Ok(new { assigned = 0 });

        var rows = wordIdsToAssign.Select(id => new UserWord
        {
            UserId = userId,
            WordId = id,
            IsLearned = false,
            CorrectCount = 0,
            WrongCount = 0,
            LastTestedAtUtc = null
        });

        await _db.UserWords.AddRangeAsync(rows);
        await _db.SaveChangesAsync();

        return Ok(new { assigned = wordIdsToAssign.Count });
    }


    // Kullanıcının şu an öğrenme listesi (IsLearned=false)
    [HttpGet("learning")]
    public async Task<IActionResult> GetLearningList()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var list = await _db.UserWords
            .Where(x => x.UserId == userId && x.IsLearned == false)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new
            {
                userWordId = x.Id,
                wordId = x.WordId,
                english = x.Word.English,
                turkish = x.Word.Turkish,
                level = (int)x.Word.Level,        // ✅ EKLENDİ
                createdAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(list);
    }


    [HttpGet("learned")]
    public async Task<IActionResult> GetLearnedList()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var list = await _db.UserWords
            .Where(x => x.UserId == userId && x.IsLearned == true)
            .OrderByDescending(x => x.LastTestedAtUtc ?? x.CreatedAtUtc)
            .Select(x => new
            {
                userWordId = x.Id,
                wordId = x.WordId,
                english = x.Word.English,
                turkish = x.Word.Turkish,
                level = (int)x.Word.Level,        // ✅ EKLENDİ
                correctCount = x.CorrectCount,
                wrongCount = x.WrongCount,
                lastTestedAtUtc = x.LastTestedAtUtc
            })
            .ToListAsync();

        return Ok(list);
    }



}
