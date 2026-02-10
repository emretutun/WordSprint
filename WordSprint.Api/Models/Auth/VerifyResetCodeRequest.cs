namespace WordSprint.Api.Models.Auth;

public class VerifyResetCodeRequest
{
    public string Email { get; set; } = default!;
    public string Code { get; set; } = default!;
}
