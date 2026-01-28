using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WordSprint.Api.Models.Profile;
using WordSprint.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using WordSprint.Infrastructure.Persistence;


namespace WordSprint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase

{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly WordSprintDbContext _db;

    public ProfileController(UserManager<ApplicationUser> userManager, WordSprintDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var fileName = string.IsNullOrWhiteSpace(user.ProfilePhotoFileName)
            ? "default.png"
            : user.ProfilePhotoFileName;

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var photoUrl = $"{baseUrl}/uploads/avatars/{fileName}";

        return Ok(new ProfileResponse
        {
            UserId = user.Id,
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            DailyWordGoal = user.DailyWordGoal,
            EstimatedLevel = user.EstimatedLevel,
            PhotoUrl = photoUrl
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        // null olan alanlara dokunma
        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.EstimatedLevel != null) user.EstimatedLevel = request.EstimatedLevel;

        if (request.DailyWordGoal.HasValue)
        {
            if (request.DailyWordGoal.Value < 1 || request.DailyWordGoal.Value > 100)
                return BadRequest("DailyWordGoal must be between 1 and 100.");

            user.DailyWordGoal = request.DailyWordGoal.Value;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return NoContent();
    }

    [HttpPost("photo")]
    public async Task<IActionResult> UploadPhoto([FromForm] UploadProfilePhotoRequest request)
    {
        var file = request.File;

        if (file == null || file.Length == 0)
            return BadRequest("File is required.");

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest("Only jpg, jpeg, png, webp allowed.");

        if (file.Length > 2 * 1024 * 1024)
            return BadRequest("Max file size is 2MB.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        // benzersiz ve çakışmasız isim (hash)
        var uniqueInput = $"{userId}-{DateTime.UtcNow.Ticks}-{Guid.NewGuid()}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(uniqueInput));
        var fileName = Convert.ToHexString(hash).ToLowerInvariant() + ext;

        var avatarsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
        Directory.CreateDirectory(avatarsDir);

        var savePath = Path.Combine(avatarsDir, fileName);

        await using (var stream = System.IO.File.Create(savePath))
        {
            await file.CopyToAsync(stream);
        }

        // eski dosyayı sil (default değilse)
        if (!string.IsNullOrWhiteSpace(user.ProfilePhotoFileName) &&
            user.ProfilePhotoFileName != "default.png")
        {
            var oldPath = Path.Combine(avatarsDir, user.ProfilePhotoFileName);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);
        }

        user.ProfilePhotoFileName = fileName;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        // client isterse hemen kullanabilsin diye URL de dönelim
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var photoUrl = $"{baseUrl}/uploads/avatars/{fileName}";

        return Ok(new { fileName, photoUrl });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var totalLearned = await _db.UserWords.CountAsync(x => x.UserId == userId && x.IsLearned);
        var totalLearning = await _db.UserWords.CountAsync(x => x.UserId == userId && !x.IsLearned);

        var totals = await _db.UserWords
            .Where(x => x.UserId == userId)
            .Select(x => new { x.CorrectCount, x.WrongCount })
            .ToListAsync();

        var totalCorrect = totals.Sum(x => x.CorrectCount);
        var totalWrong = totals.Sum(x => x.WrongCount);

        var totalAnswered = totalCorrect + totalWrong;
        var successRate = totalAnswered == 0 ? 0 : (double)totalCorrect / totalAnswered * 100.0;

        // Şimdilik "bugün learned" = bugün içinde LastTestedAt ile learned olanlar gibi approx
        // Daha doğru tracking'i ileride ekleyeceğiz (LearnedAtUtc alanı ile)
        var todayUtc = DateTime.UtcNow.Date;
        var todayLearned = await _db.UserWords.CountAsync(x =>
            x.UserId == userId &&
            x.IsLearned &&
            x.LastTestedAtUtc != null &&
            x.LastTestedAtUtc.Value >= todayUtc);

        return Ok(new ProfileStatsResponse
        {
            TotalLearned = totalLearned,
            TotalLearning = totalLearning,
            TotalCorrect = totalCorrect,
            TotalWrong = totalWrong,
            SuccessRate = Math.Round(successRate, 2),
            TodayLearned = todayLearned
        });
    }


}
