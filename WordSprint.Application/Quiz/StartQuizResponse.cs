namespace WordSprint.Application.Models.Quiz;

public class StartQuizResponse
{
    public List<QuizQuestionDto> Questions { get; set; } = new();
}
