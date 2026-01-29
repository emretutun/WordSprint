namespace WordSprint.Api.Models.Auth;

public class ResetPasswordRequest
{
    public string UserId { get; set; } = default!;
    public string Token { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}
