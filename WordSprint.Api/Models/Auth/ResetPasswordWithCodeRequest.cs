namespace WordSprint.Api.Models.Auth;

public class ResetPasswordWithCodeRequest
{
    public string Email { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}
