using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordSprint.Application.Models.Quiz;

namespace WordSprint.Application.Services.Quiz;

public static class QuizScoring
{
    public static SubmitQuizResponse Score(SubmitQuizRequest request, List<QuizWordSnapshot> words)
    {
        if (request.Answers == null || request.Answers.Count == 0)
            throw new ArgumentException("Answers required.");

        var map = words.ToDictionary(x => x.WordId);

        int correct = 0;
        var items = new List<QuizResultItem>();

        foreach (var ans in request.Answers)
        {
            if (!map.TryGetValue(ans.WordId, out var w))
                continue;

            string expected = ans.Mode switch
            {
                QuizMode.TrToEnTyping or QuizMode.TrToEnMultipleChoice => w.English,
                QuizMode.EnToTrTyping or QuizMode.EnToTrMultipleChoice => w.Turkish,
                _ => ""
            };

            string prompt = ans.Mode switch
            {
                QuizMode.TrToEnTyping or QuizMode.TrToEnMultipleChoice => w.Turkish,
                QuizMode.EnToTrTyping or QuizMode.EnToTrMultipleChoice => w.English,
                _ => ""
            };

            bool isCorrect = Normalize(ans.Answer ?? "") == Normalize(expected);
            if (isCorrect) correct++;

            items.Add(new QuizResultItem
            {
                WordId = w.WordId,
                IsCorrect = isCorrect,
                Prompt = prompt,
                UserAnswer = ans.Answer ?? "",
                CorrectAnswer = expected,
                Level = w.Level
            });
        }

        int total = request.Answers.Count;
        double rate = total == 0 ? 0 : (double)correct / total * 100.0;
        bool passed = rate >= 70.0;

        return new SubmitQuizResponse
        {
            Total = total,
            Correct = correct,
            Wrong = total - correct,
            SuccessRate = Math.Round(rate, 2),
            Passed = passed,
            Items = items
        };
    }

    private static string Normalize(string s) => (s ?? "").Trim().ToLowerInvariant();
}

public record QuizWordSnapshot(int WordId, string Turkish, string English, int Level);
