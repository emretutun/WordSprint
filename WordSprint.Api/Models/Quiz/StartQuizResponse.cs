namespace WordSprint.Api.Models.Quiz;

public class StartQuizResponse
{
    public List<QuizQuestionDto> Questions { get; set; } = new();
}
