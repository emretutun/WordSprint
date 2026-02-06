using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WordSprint.Api.Models.Quiz;
using WordSprint.Core.Entities;
using WordSprint.Infrastructure.Persistence;

namespace WordSprint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly WordSprintDbContext _db;

    public QuizController(WordSprintDbContext db)
    {
        _db = db;
    }

    
    [HttpPost("start")]
    public async Task<IActionResult> Start(
        [FromQuery] int count = 10,
        [FromQuery] QuizMode mode = QuizMode.TrToEnTyping)
    {
        if (count <= 0 || count > 50)
            return BadRequest("count must be between 1 and 50.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var words = await _db.UserWords
            .Where(x => x.UserId == userId && x.IsLearned == false)
            .OrderBy(x => Guid.NewGuid())
            .Take(count)
            .Select(x => x.Word)
            .ToListAsync();

        if (words.Count == 0)
            return Ok(new StartQuizResponse());

        var questions = new List<QuizQuestionDto>();

        foreach (var w in words)
        {
            // ✅ Her soru için mod seç: Mixed ise random, değilse sabit
            var actualMode = mode == QuizMode.Mixed
                ? PickRandomMode()
                : mode;

            var q = new QuizQuestionDto
            {
                WordId = w.Id,
                Level = (int)w.Level,
                Mode = actualMode
            };

            switch (actualMode)
            {
                case QuizMode.TrToEnTyping:
                    q.Prompt = w.Turkish;
                    q.ExpectedLanguage = "EN";
                    break;

                case QuizMode.EnToTrTyping:
                    q.Prompt = w.English;
                    q.ExpectedLanguage = "TR";
                    break;

                case QuizMode.TrToEnMultipleChoice:
                    {
                        q.Prompt = w.Turkish;
                        q.ExpectedLanguage = "EN";

                        var pool = await _db.Words
                            .OrderBy(x => Guid.NewGuid())
                            .Take(200)
                            .Select(x => x.English)
                            .ToListAsync();

                        q.Choices = BuildChoices(correct: w.English, pool: pool);
                        break;
                    }

                case QuizMode.EnToTrMultipleChoice:
                    {
                        q.Prompt = w.English;
                        q.ExpectedLanguage = "TR";

                        var pool = await _db.Words
                            .OrderBy(x => Guid.NewGuid())
                            .Take(200)
                            .Select(x => x.Turkish)
                            .ToListAsync();

                        q.Choices = BuildChoices(correct: w.Turkish, pool: pool);
                        break;
                    }
            }

            questions.Add(q);
        }

        return Ok(new StartQuizResponse { Questions = questions });
    }


    // Quiz cevaplarını alır, puanlar, >=70 ise ilgili kelimeleri learned yapar
    [HttpPost("submit")]
    public async Task<IActionResult> Submit(SubmitQuizRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (request.Answers == null || request.Answers.Count == 0)
            return BadRequest("Answers required.");

        var wordIds = request.Answers.Select(a => a.WordId).Distinct().ToList();

        // Sadece bu kullanıcıya ait kelimeleri getir
        var userWords = await _db.UserWords
            .Where(x => x.UserId == userId && wordIds.Contains(x.WordId))
            .Include(x => x.Word)
            .ToListAsync();

        if (userWords.Count == 0)
            return BadRequest("No matching words found for this user.");

        int correct = 0;
        int newlyLearned = 0;
        var items = new List<QuizResultItem>();

        foreach (var ans in request.Answers)
        {
            var uw = userWords.FirstOrDefault(x => x.WordId == ans.WordId);
            if (uw == null) continue;

            // Modlara göre beklenen cevabı ve sorulan soruyu belirle
            string expected = ans.Mode switch
            {
                QuizMode.TrToEnTyping or QuizMode.TrToEnMultipleChoice => uw.Word.English,
                QuizMode.EnToTrTyping or QuizMode.EnToTrMultipleChoice => uw.Word.Turkish,
                _ => ""
            };

            string prompt = ans.Mode switch
            {
                QuizMode.TrToEnTyping or QuizMode.TrToEnMultipleChoice => uw.Word.Turkish,
                QuizMode.EnToTrTyping or QuizMode.EnToTrMultipleChoice => uw.Word.English,
                _ => ""
            };

            bool isCorrect = Normalize(ans.Answer ?? "") == Normalize(expected);

            if (isCorrect)
            {
                correct++;
                uw.CorrectCount += 1;
            }
            else
            {
                uw.WrongCount += 1;
            }

            uw.LastTestedAtUtc = DateTime.UtcNow;

            items.Add(new QuizResultItem
            {
                WordId = uw.WordId,
                IsCorrect = isCorrect,
                Prompt = prompt,
                UserAnswer = ans.Answer ?? "",
                CorrectAnswer = expected,
                Level = (int)uw.Word.Level
            });
        }

        // Puanlama ve Başarı Durumu
        int total = request.Answers.Count;
        double rate = total == 0 ? 0 : (double)correct / total * 100.0;
        bool passed = rate >= 70.0;

        // Eğer quiz geçildiyse kelimeleri "öğrenildi" olarak işaretle
        if (passed)
        {
            foreach (var uw in userWords)
            {
                if (!uw.IsLearned)
                {
                    uw.IsLearned = true;
                    newlyLearned++;
                }
            }
        }

        // Günlük Aktivite Güncelleme
        var todayUtc = DateTime.UtcNow.Date;
        var activity = await _db.UserDailyActivities
            .FirstOrDefaultAsync(x => x.UserId == userId && x.DayUtc == todayUtc);

        if (activity == null)
        {
            activity = new UserDailyActivity
            {
                UserId = userId,
                DayUtc = todayUtc,
                LearnedCount = 0,
                QuizCount = 0,
                UpdatedAtUtc = DateTime.UtcNow
            };
            _db.UserDailyActivities.Add(activity);
        }

        activity.QuizCount += 1;
        if (passed && newlyLearned > 0)
        {
            activity.LearnedCount += newlyLearned;
        }
        activity.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new SubmitQuizResponse
        {
            Total = total,
            Correct = correct,
            Wrong = total - correct,
            SuccessRate = Math.Round(rate, 2),
            Passed = passed,
            Items = items
        });
    }

    private static List<string> BuildChoices(string correct, List<string> pool)
    {
        // correct + 3 yanlış (tekrarsız)
        var choices = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { correct };

        foreach (var opt in pool.OrderBy(_ => Guid.NewGuid()))
        {
            if (choices.Count >= 4) break;
            if (!string.Equals(opt, correct, StringComparison.OrdinalIgnoreCase))
                choices.Add(opt);
        }

        // karıştır
        return choices.OrderBy(_ => Guid.NewGuid()).ToList();
    }

    private static string Normalize(string s)
    {
        return (s ?? "")
            .Trim()
            .ToLowerInvariant();
    }

    // Learned kelimelerden tekrar quiz'i (spaced repetition için temel)
    [HttpPost("repeat/start")]
    public async Task<IActionResult> StartRepeat(
    [FromQuery] int count = 10,
    [FromQuery] QuizMode mode = QuizMode.Mixed) // Default olarak Mixed yaptık
    {
        if (count <= 0 || count > 50)
            return BadRequest("count must be between 1 and 50.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        // Sadece öğrenilmiş kelimeleri çek
        var words = await _db.UserWords
            .Where(x => x.UserId == userId && x.IsLearned == true)
            .OrderBy(x => Guid.NewGuid())
            .Take(count)
            .Select(x => x.Word)
            .ToListAsync();

        if (words.Count == 0)
            return Ok(new StartQuizResponse());

        var questions = new List<QuizQuestionDto>();

        foreach (var w in words)
        {
            // ✅ KRİTİK NOKTA: Karışık mod seçildiyse her soru için rastgele mod ata
            var actualMode = mode == QuizMode.Mixed ? PickRandomMode() : mode;

            var q = new QuizQuestionDto
            {
                WordId = w.Id,
                Level = (int)w.Level,
                Mode = actualMode
            };

            // Mod tipine göre Prompt ve Language ayarlarını yap
            switch (actualMode)
            {
                case QuizMode.TrToEnTyping:
                    q.Prompt = w.Turkish;
                    q.ExpectedLanguage = "EN";
                    break;

                case QuizMode.EnToTrTyping:
                    q.Prompt = w.English;
                    q.ExpectedLanguage = "TR";
                    break;

                case QuizMode.TrToEnMultipleChoice:
                    q.Prompt = w.Turkish;
                    q.ExpectedLanguage = "EN";
                    // Çoktan seçmeli için havuzdan şıkları çek (BuildChoices metodun varsa kullan)
                    q.Choices = await BuildChoicesAsync(w.English, isEnglish: true);
                    break;

                case QuizMode.EnToTrMultipleChoice:
                    q.Prompt = w.English;
                    q.ExpectedLanguage = "TR";
                    q.Choices = await BuildChoicesAsync(w.Turkish, isEnglish: false);
                    break;
            }

            questions.Add(q);
        }

        return Ok(new StartQuizResponse { Questions = questions });
    }

    private async Task<List<string>> BuildChoicesAsync(string correct, bool isEnglish)
    {
        var pool = isEnglish
            ? await _db.Words.OrderBy(x => Guid.NewGuid()).Take(200).Select(x => x.English).ToListAsync()
            : await _db.Words.OrderBy(x => Guid.NewGuid()).Take(200).Select(x => x.Turkish).ToListAsync();

        var choices = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { correct };

        foreach (var opt in pool.OrderBy(_ => Guid.NewGuid()))
        {
            if (choices.Count >= 4) break;
            if (!string.Equals(opt, correct, StringComparison.OrdinalIgnoreCase))
                choices.Add(opt);
        }

        return choices.OrderBy(_ => Guid.NewGuid()).ToList();
    }
    [HttpPost("repeat/submit")]
    public async Task<IActionResult> SubmitRepeat(SubmitQuizRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (request.Answers == null || request.Answers.Count == 0)
            return BadRequest("Answers required.");

        var wordIds = request.Answers.Select(a => a.WordId).Distinct().ToList();

        // SADECE daha önce öğrenilmiş (learned) kelimeler üzerinde çalış
        var userWords = await _db.UserWords
            .Where(x => x.UserId == userId && x.IsLearned == true && wordIds.Contains(x.WordId))
            .Include(x => x.Word)
            .ToListAsync();

        if (userWords.Count == 0)
            return BadRequest("No matching learned words found for this user.");

        int correct = 0;
        var items = new List<QuizResultItem>();

        foreach (var ans in request.Answers)
        {
            var uw = userWords.FirstOrDefault(x => x.WordId == ans.WordId);
            if (uw == null) continue;

            // Karışık mod desteği: Eğer cevap bazlı mode gönderilmemişse genel mode'a bak
            var currentMode = ans.Mode != QuizMode.Mixed ? ans.Mode : request.Mode;

            // Beklenen cevabı mod bazlı belirle
            var expected = currentMode switch
            {
                QuizMode.TrToEnTyping or QuizMode.TrToEnMultipleChoice => uw.Word.English,
                QuizMode.EnToTrTyping or QuizMode.EnToTrMultipleChoice => uw.Word.Turkish,
                _ => ""
            };

            // Soru metnini (Prompt) sonuç ekranında göstermek için belirle
            var prompt = currentMode switch
            {
                QuizMode.TrToEnTyping or QuizMode.TrToEnMultipleChoice => uw.Word.Turkish,
                QuizMode.EnToTrTyping or QuizMode.EnToTrMultipleChoice => uw.Word.English,
                _ => ""
            };

            bool isCorrect = Normalize(ans.Answer ?? "") == Normalize(expected);

            if (isCorrect)
            {
                correct++;
                uw.CorrectCount += 1;
            }
            else
            {
                uw.WrongCount += 1;
                // Yanlış bilirse kelimeyi tekrar öğrenme (unlearned) statüsüne çek
                uw.IsLearned = false;
            }

            uw.LastTestedAtUtc = DateTime.UtcNow;

            items.Add(new QuizResultItem
            {
                WordId = uw.WordId,
                IsCorrect = isCorrect,
                Prompt = prompt,
                UserAnswer = ans.Answer ?? "",
                CorrectAnswer = expected,
                Level = (int)uw.Word.Level
            });
        }

        int total = request.Answers.Count;
        int wrong = total - correct;
        double rate = total == 0 ? 0 : (double)correct / total * 100.0;

        await _db.SaveChangesAsync();

        return Ok(new SubmitQuizResponse
        {
            Total = total,
            Correct = correct,
            Wrong = wrong,
            SuccessRate = Math.Round(rate, 2),
            Passed = true, // Repeat ekranında passed kontrolü yapmıyoruz, her zaman true döner
            Items = items
        });
    }

    private static QuizMode PickRandomMode()
    {
        var modes = new[]
        {
        QuizMode.TrToEnTyping,
        QuizMode.EnToTrTyping,
        QuizMode.TrToEnMultipleChoice,
        QuizMode.EnToTrMultipleChoice
    };

        return modes[Random.Shared.Next(modes.Length)];
    }



}

