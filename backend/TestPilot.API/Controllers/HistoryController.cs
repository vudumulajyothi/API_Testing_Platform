using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestPilot.Application.Interfaces;

namespace TestPilot.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly IHistoryService _historyService;

    public HistoryController(IHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? method = null,
        [FromQuery] int? statusCode = null)
    {
        var userId = GetCurrentUserId();
        var result = await _historyService.GetUserHistoryAsync(userId, page, pageSize, method, statusCode);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> ClearHistory()
    {
        var userId = GetCurrentUserId();
        var cleared = await _historyService.ClearUserHistoryAsync(userId);

        if (!cleared)
            return NotFound(new { message = "No history records to delete." });

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user claim in token.");
        }

        return userId;
    }
}
