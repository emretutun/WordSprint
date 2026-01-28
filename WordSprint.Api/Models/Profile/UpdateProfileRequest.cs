namespace WordSprint.Api.Models.Profile;

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? DailyWordGoal { get; set; }
    public string? EstimatedLevel { get; set; }
}
