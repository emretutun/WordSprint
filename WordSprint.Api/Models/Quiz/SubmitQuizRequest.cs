namespace WordSprint.Api.Models.Quiz;

public class SubmitQuizRequest
{
    public QuizMode Mode { get; set; }

    // Her soru için user cevabı
    public List<SubmitQuizAnswer> Answers { get; set; } = new();
}

public class SubmitQuizAnswer
{
    public int WordId { get; set; }
    public string Answer { get; set; } = default!;
}
