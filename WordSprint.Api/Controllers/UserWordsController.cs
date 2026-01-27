using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WordSprint.Core.Entities;
using WordSprint.Infrastructure.Persistence;

namespace WordSprint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserWordsController : ControllerBase
{
    private readonly WordSprintDbContext _db;

    public UserWordsController(WordSprintDbContext db)
    {
        _db = db;
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

        // Kullanıcının zaten bildiği/atanmış kelime id’leri
        var existingWordIds = await _db.UserWords
            .Where(x => x.UserId == userId)
            .Select(x => x.WordId)
            .ToListAsync();

        // Mevcut olmayanlardan random seç
        var newWords = await _db.Words
            .Where(w => !existingWordIds.Contains(w.Id))
            .OrderBy(x => Guid.NewGuid())
            .Take(count)
            .ToListAsync();

        if (newWords.Count == 0)
            return Ok(new List<object>());

        // UserWord kayıtlarını oluştur
        var userWords = newWords.Select(w => new UserWord
        {
            UserId = userId,
            WordId = w.Id,
            IsLearned = false
        }).ToList();

        await _db.UserWords.AddRangeAsync(userWords);
        await _db.SaveChangesAsync();

        // kullanıcıya dön
        var response = newWords.Select(w => new { w.Id, w.English, w.Turkish }).ToList();
        return Ok(response);
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
                correctCount = x.CorrectCount,
                wrongCount = x.WrongCount,
                lastTestedAtUtc = x.LastTestedAtUtc
            })
            .ToListAsync();

        return Ok(list);
    }


}
