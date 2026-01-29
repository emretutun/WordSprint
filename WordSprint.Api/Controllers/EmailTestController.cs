using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WordSprint.Api.Services;

namespace WordSprint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailTestController : ControllerBase
{
    private readonly EmailService _email;

    public EmailTestController(EmailService email)
    {
        _email = email;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromQuery] string to)
    {
        await _email.SendAsync(
            toEmail: to,
            subject: "WordSprint SMTP Test",
            htmlBody: "<h3>SMTP works ✅</h3><p>This is a test email from WordSprint.</p>"
        );

        return Ok("sent");
    }
}
