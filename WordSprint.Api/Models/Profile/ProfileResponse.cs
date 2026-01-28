namespace WordSprint.Api.Models.Profile;

public class ProfileResponse
{
    public string UserId { get; set; } = default!;
    public string Email { get; set; } = default!;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int DailyWordGoal { get; set; }
    public string? EstimatedLevel { get; set; }
    public string PhotoUrl { get; set; } = default!;

}
