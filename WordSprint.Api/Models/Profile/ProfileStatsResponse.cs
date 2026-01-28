namespace WordSprint.Api.Models.Profile;

public class ProfileStatsResponse
{
    public int TotalLearned { get; set; }
    public int TotalLearning { get; set; }

    public int TotalCorrect { get; set; }
    public int TotalWrong { get; set; }

    public double SuccessRate { get; set; } // 0-100

    public int TodayLearned { get; set; } // UTC bazlı (şimdilik)
}
