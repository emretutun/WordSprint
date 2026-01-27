namespace WordSprint.Api.Models.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
}