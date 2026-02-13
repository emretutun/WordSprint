using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using WordSprint.Application.Models.Quiz;
using WordSprint.Application.Services.Quiz;
using Xunit;

namespace WordSprint.Tests.Quiz;

public class QuizScoringTests
{
    [Fact]
    public void Score_WhenSuccessRateAtLeast70_ShouldPass()
    {
        // Arrange
        var words = new List<QuizWordSnapshot>
        {
            new(1, "elma", "apple", 1),
            new(2, "araba", "car", 1),
            new(3, "ev", "house", 1),
            new(4, "su", "water", 1),
            new(5, "kitap", "book", 1),
            new(6, "kalem", "pen", 1),
            new(7, "masa", "table", 1),
            new(8, "kedi", "cat", 1),
            new(9, "köpek", "dog", 1),
            new(10, "çay", "tea", 1),
        };

        // 7 doğru / 10 => %70 => passed true
        var request = new SubmitQuizRequest
        {
            Answers = new List<SubmitQuizAnswer>
            {
                new() { WordId = 1, Mode = QuizMode.TrToEnTyping, Answer = "apple" },
                new() { WordId = 2, Mode = QuizMode.TrToEnTyping, Answer = "car" },
                new() { WordId = 3, Mode = QuizMode.TrToEnTyping, Answer = "house" },
                new() { WordId = 4, Mode = QuizMode.TrToEnTyping, Answer = "water" },
                new() { WordId = 5, Mode = QuizMode.TrToEnTyping, Answer = "book" },
                new() { WordId = 6, Mode = QuizMode.TrToEnTyping, Answer = "pen" },
                new() { WordId = 7, Mode = QuizMode.TrToEnTyping, Answer = "table" },

                new() { WordId = 8, Mode = QuizMode.TrToEnTyping, Answer = "WRONG" },
                new() { WordId = 9, Mode = QuizMode.TrToEnTyping, Answer = "WRONG" },
                new() { WordId = 10, Mode = QuizMode.TrToEnTyping, Answer = "WRONG" },
            }
        };

        // Act
        var result = QuizScoring.Score(request, words);

        // Assert
        result.Total.Should().Be(10);
        result.Correct.Should().Be(7);
        result.Wrong.Should().Be(3);
        result.SuccessRate.Should().Be(70.00);
        result.Passed.Should().BeTrue();
        result.Items.Should().HaveCount(10);
    }

    [Fact]
    public void Score_WhenSuccessRateBelow70_ShouldFail()
    {
        // Arrange
        var words = new List<QuizWordSnapshot>
    {
        new(1, "elma", "apple", 1),
        new(2, "araba", "car", 1),
        new(3, "ev", "house", 1),
        new(4, "su", "water", 1),
        new(5, "kitap", "book", 1),
        new(6, "kalem", "pen", 1),
        new(7, "masa", "table", 1),
        new(8, "kedi", "cat", 1),
        new(9, "köpek", "dog", 1),
        new(10, "çay", "tea", 1),
    };

        // 6 doğru / 10 => %60 => failed
        var request = new SubmitQuizRequest
        {
            Answers = new List<SubmitQuizAnswer>
        {
            new() { WordId = 1, Mode = QuizMode.TrToEnTyping, Answer = "apple" },
            new() { WordId = 2, Mode = QuizMode.TrToEnTyping, Answer = "car" },
            new() { WordId = 3, Mode = QuizMode.TrToEnTyping, Answer = "house" },
            new() { WordId = 4, Mode = QuizMode.TrToEnTyping, Answer = "water" },
            new() { WordId = 5, Mode = QuizMode.TrToEnTyping, Answer = "book" },
            new() { WordId = 6, Mode = QuizMode.TrToEnTyping, Answer = "pen" },

            new() { WordId = 7, Mode = QuizMode.TrToEnTyping, Answer = "WRONG" },
            new() { WordId = 8, Mode = QuizMode.TrToEnTyping, Answer = "WRONG" },
            new() { WordId = 9, Mode = QuizMode.TrToEnTyping, Answer = "WRONG" },
            new() { WordId = 10, Mode = QuizMode.TrToEnTyping, Answer = "WRONG" },
        }
        };

        // Act
        var result = QuizScoring.Score(request, words);

        // Assert
        result.SuccessRate.Should().Be(60.00);
        result.Passed.Should().BeFalse();
    }


    [Fact]
    public void Score_ShouldBeCaseInsensitive_AndTrimSpaces()
    {
        // Arrange
        var words = new List<QuizWordSnapshot>
    {
        new(1, "elma", "apple", 1),
    };

        var request = new SubmitQuizRequest
        {
            Answers = new List<SubmitQuizAnswer>
        {
            new()
            {
                WordId = 1,
                Mode = QuizMode.TrToEnTyping,
                Answer = "  APPLE  "
            }
        }
        };

        // Act
        var result = QuizScoring.Score(request, words);

        // Assert
        result.Correct.Should().Be(1);
        result.Passed.Should().BeTrue();
    }

    [Fact]
    public void Score_WhenAnswersNull_ShouldThrow()
    {
        // Arrange
        var words = new List<QuizWordSnapshot>
    {
        new(1, "elma", "apple", 1),
    };

        var request = new SubmitQuizRequest
        {
            Answers = null!
        };

        // Act
        var act = () => QuizScoring.Score(request, words);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Score_WhenAnswersEmpty_ShouldThrow()
    {
        // Arrange
        var words = new List<QuizWordSnapshot>
    {
        new(1, "elma", "apple", 1),
    };

        var request = new SubmitQuizRequest
        {
            Answers = new List<SubmitQuizAnswer>()
        };

        // Act
        var act = () => QuizScoring.Score(request, words);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Score_ShouldUseCorrectLanguage_WhenModeIsEnToTr()
    {
        // Arrange
        var words = new List<QuizWordSnapshot>
    {
        new(1, "elma", "apple", 1),
    };

        var request = new SubmitQuizRequest
        {
            Answers = new List<SubmitQuizAnswer>
        {
            new()
            {
                WordId = 1,
                Mode = QuizMode.EnToTrTyping,
                Answer = "elma"
            }
        }
        };

        // Act
        var result = QuizScoring.Score(request, words);

        // Assert
        result.Correct.Should().Be(1);
        result.Passed.Should().BeTrue();
    }
    [Fact]
    public void Score_WhenWordNotFound_ShouldSkipItemButKeepTotal()
    {
        // Arrange
        var words = new List<QuizWordSnapshot>
    {
        new(1, "elma", "apple", 1),
    };

        var request = new SubmitQuizRequest
        {
            Answers = new List<SubmitQuizAnswer>
        {
            new() { WordId = 1, Mode = QuizMode.TrToEnTyping, Answer = "apple" },
            new() { WordId = 999, Mode = QuizMode.TrToEnTyping, Answer = "anything" }, // yok
        }
        };

        // Act
        var result = QuizScoring.Score(request, words);

        // Assert
        result.Total.Should().Be(2);
        result.Correct.Should().Be(1);
        result.Items.Should().HaveCount(1); // sadece bulunan word için item üretilir
    }


    [Fact]
    public void Score_WhenDuplicateWordIds_ShouldScoreEachAnswerSeparately()
    {
        // Arrange
        var words = new List<QuizWordSnapshot>
    {
        new(1, "elma", "apple", 1),
    };

        var request = new SubmitQuizRequest
        {
            Answers = new List<SubmitQuizAnswer>
        {
            new() { WordId = 1, Mode = QuizMode.TrToEnTyping, Answer = "apple" }, // doğru
            new() { WordId = 1, Mode = QuizMode.TrToEnTyping, Answer = "wrong" }, // yanlış
        }
        };

        // Act
        var result = QuizScoring.Score(request, words);

        // Assert
        result.Total.Should().Be(2);
        result.Correct.Should().Be(1);
        result.Wrong.Should().Be(1);
        result.Items.Should().HaveCount(2);
    }








}
