using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordSprint.Infrastructure.Persistence;

namespace WordSprint.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WordsController : ControllerBase
{
    private readonly WordSprintDbContext _db;

    public WordsController(WordSprintDbContext db)
    {
        _db = db;
    }

    // Şimdilik herkes çekebilsin (istersen sonra [Authorize] yaparız)
    [HttpGet("random")]
    public async Task<IActionResult> GetRandom([FromQuery] int count = 10)
    {
        if (count <= 0 || count > 50)
            return BadRequest("count must be between 1 and 50.");

        var words = await _db.Words
            .OrderBy(x => Guid.NewGuid())
            .Take(count)
            .Select(x => new { x.Id, x.English, x.Turkish })
            .ToListAsync();

        return Ok(words);
    }
}
