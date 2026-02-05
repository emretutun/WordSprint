namespace WordSprint.Api.Models.Profile;

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? DailyWordGoal { get; set; }
    public short? Level { get; set; } // 0..5 (A1..C2)
}
