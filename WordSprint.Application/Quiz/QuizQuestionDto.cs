namespace WordSprint.Application.Models.Quiz;

public class QuizQuestionDto
{
    public int WordId { get; set; }
    public QuizMode Mode { get; set; }
    public int Level { get; set; }

    public string Prompt { get; set; } = default!; // soru metni (TR veya EN)
    public List<string>? Choices { get; set; }     // çoktan seçmeli ise dolu

    // client tarafı için
    public string? ExpectedLanguage { get; set; }  // "EN" veya "TR"
}
