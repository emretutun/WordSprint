using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WordSprint.Api.Models.Auth;
using WordSprint.Api.Services;
using WordSprint.Infrastructure.Identity;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace WordSprint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwt;
    private readonly EmailService _email;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwt,
        EmailService email)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwt = jwt;
        _email = email;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return BadRequest("Email already exists.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        // Email confirmation token üret
        var confirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // URL-safe token
        var tokenBytes = Encoding.UTF8.GetBytes(confirmToken);
        var encodedToken = WebEncoders.Base64UrlEncode(tokenBytes);

        // confirm link
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var confirmUrl = $"{baseUrl}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        var html = $@"
<h3>WordSprint - Email Confirmation</h3>
<p>Hesabını onaylamak için linke tıkla:</p>
<p><a href='{HtmlEncoder.Default.Encode(confirmUrl)}'>Emaili Onayla</a></p>
<p>Eğer bu kaydı sen yapmadıysan görmezden gelebilirsin.</p>";

        await _email.SendAsync(user.Email!, "WordSprint - Email Confirmation", html);

        return Ok(new { message = "Registered. Please confirm your email." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized("Invalid credentials.");
        if (!user.EmailConfirmed)
            return Unauthorized("Email not confirmed.");

        var signIn = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!signIn.Succeeded)
            return Unauthorized("Invalid credentials.");

        var (token, expires) = _jwt.CreateAccessToken(user);
        return Ok(new AuthResponse { AccessToken = token, ExpiresAtUtc = expires });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return BadRequest("Invalid user.");

        var decodedBytes = WebEncoders.Base64UrlDecode(token);
        var decodedToken = Encoding.UTF8.GetString(decodedBytes);

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok("Email confirmed ✅");
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Güvenlik: email var/yok fark ettirme (her zaman aynı mesaj)
        if (user == null)
            return Ok(new { message = "If the email exists, a reset link has been sent." });

        // Reset token üret
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        // URL-safe yap
        var tokenBytes = Encoding.UTF8.GetBytes(resetToken);
        var encodedToken = WebEncoders.Base64UrlEncode(tokenBytes);

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var resetUrl = $"{baseUrl}/api/auth/reset-password?userId={user.Id}&token={encodedToken}";

        var html = $@"
<h3>WordSprint - Password Reset</h3>
<p>Şifreni sıfırlamak için linke tıkla:</p>
<p><a href='{HtmlEncoder.Default.Encode(resetUrl)}'>Şifremi Sıfırla</a></p>
<p>Eğer bu isteği sen yapmadıysan görmezden gelebilirsin.</p>";

        await _email.SendAsync(user.Email!, "WordSprint - Password Reset", html);

        return Ok(new { message = "If the email exists, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return BadRequest("Invalid user.");

        // URL-safe token decode
        var decodedBytes = WebEncoders.Base64UrlDecode(request.Token);
        var decodedToken = Encoding.UTF8.GetString(decodedBytes);

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok(new { message = "Password has been reset ✅" });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Unauthorized();

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok(new { message = "Password changed ✅" });
    }

}
