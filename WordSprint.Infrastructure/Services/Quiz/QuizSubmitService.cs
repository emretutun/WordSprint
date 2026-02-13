using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WordSprint.Application.Models.Quiz;
using WordSprint.Core.Entities;
using WordSprint.Infrastructure.Persistence;


namespace WordSprint.Infrastructure.Services.Quiz;

public class QuizSubmitService
{
    private readonly WordSprintDbContext _db;

    public QuizSubmitService(WordSprintDbContext db)
    {
        _db = db;
    }

    public async Task<SubmitQuizResponse> SubmitAsync(string userId, SubmitQuizRequest request)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("userId is required.", nameof(userId));

        if (request.Answers == null || request.Answers.Count == 0)
            throw new ArgumentException("Answers required.", nameof(request));

        var wordIds = request.Answers.Select(a => a.WordId).Distinct().ToList();

        var userWords = await _db.UserWords
            .Where(x => x.UserId == userId && wordIds.Contains(x.WordId))
            .Include(x => x.Word)
            .ToListAsync();

        if (userWords.Count == 0)
            throw new InvalidOperationException("No matching words found for this user.");

        // DB'den snapshot çıkar, scoring'i kullan
        var snapshots = userWords.Select(uw =>
            new WordSprint.Application.Services.Quiz.QuizWordSnapshot(
                uw.WordId,
                uw.Word.Turkish,
                uw.Word.English,
                (int)uw.Word.Level
            )).ToList();

        var scoringResult = WordSprint.Application.Services.Quiz.QuizScoring.Score(request, snapshots);

        // Counters + learned update
        // (orijinal controller mantığınla aynı)
        foreach (var ans in request.Answers)
        {
            var uw = userWords.FirstOrDefault(x => x.WordId == ans.WordId);
            if (uw == null) continue;

            var expected = ans.Mode switch
            {
                QuizMode.TrToEnTyping or QuizMode.TrToEnMultipleChoice => uw.Word.English,
                QuizMode.EnToTrTyping or QuizMode.EnToTrMultipleChoice => uw.Word.Turkish,
                _ => ""
            };

            bool isCorrect = Normalize(ans.Answer ?? "") == Normalize(expected);

            if (isCorrect) uw.CorrectCount += 1;
            else uw.WrongCount += 1;

            uw.LastTestedAtUtc = DateTime.UtcNow;
        }

        int newlyLearned = 0;
        if (scoringResult.Passed)
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

        // DailyActivity
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
        if (scoringResult.Passed && newlyLearned > 0)
            activity.LearnedCount += newlyLearned;

        activity.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return scoringResult;
    }

    private static string Normalize(string s) => (s ?? "").Trim().ToLowerInvariant();
}
