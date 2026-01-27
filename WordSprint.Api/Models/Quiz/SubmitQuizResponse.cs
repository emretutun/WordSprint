namespace WordSprint.Api.Models.Quiz;

public class SubmitQuizResponse
{
    public int Total { get; set; }
    public int Correct { get; set; }
    public int Wrong { get; set; }
    public double SuccessRate { get; set; } // 0-100
    public bool Passed { get; set; }

    public List<QuizResultItem> Items { get; set; } = new();
}

public class QuizResultItem
{
    public int WordId { get; set; }
    public bool IsCorrect { get; set; }
    public string CorrectAnswer { get; set; } = default!;
}
