using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using WordSprint.Application.Models.Quiz;
using WordSprint.Infrastructure.Services.Quiz;
using WordSprint.Core.Entities;
using Xunit;
using WordSprint.Core.Enums;

namespace WordSprint.Tests.Integration;

public class QuizSubmitServiceTests
{
    [Fact]
    public async Task SubmitAsync_WhenPassed_ShouldMarkWordsLearned_AndCreateDailyActivity()
    {
        // Arrange
        using var db = SqliteDbContextFactory.Create();

        var userId = "user-1";

        // Words
        var w1 = new Word { Turkish = "elma", English = "apple", Level = CeLevel.A1};
        var w2 = new Word { Turkish = "araba", English = "car", Level = CeLevel.A1 };
        db.Words.AddRange(w1, w2);
        await db.SaveChangesAsync();

        // UserWords (not learned)
        db.UserWords.AddRange(
            new UserWord { UserId = userId, WordId = w1.Id, IsLearned = false, CorrectCount = 0, WrongCount = 0 },
            new UserWord { UserId = userId, WordId = w2.Id, IsLearned = false, CorrectCount = 0, WrongCount = 0 }
        );
        await db.SaveChangesAsync();

        var service = new QuizSubmitService(db);

        var request = new SubmitQuizRequest
        {
            Answers = new List<SubmitQuizAnswer>
            {
                new() { WordId = w1.Id, Mode = QuizMode.TrToEnTyping, Answer = "apple" },
                new() { WordId = w2.Id, Mode = QuizMode.TrToEnTyping, Answer = "car" }
            }
        };

        // Act
        var result = await service.SubmitAsync(userId, request);

        // Assert (response)
        result.Passed.Should().BeTrue();
        result.Correct.Should().Be(2);

        // Assert (db side effects)
        var userWords = db.UserWords.Where(x => x.UserId == userId).ToList();
        userWords.Should().HaveCount(2);
        userWords.All(x => x.IsLearned).Should().BeTrue();
        userWords.Sum(x => x.CorrectCount).Should().Be(2);
        userWords.Sum(x => x.WrongCount).Should().Be(0);

        var today = DateTime.UtcNow.Date;
        var activity = db.UserDailyActivities.SingleOrDefault(x => x.UserId == userId && x.DayUtc == today);
        activity.Should().NotBeNull();
        activity!.QuizCount.Should().Be(1);
        activity.LearnedCount.Should().Be(2);
    }
}
