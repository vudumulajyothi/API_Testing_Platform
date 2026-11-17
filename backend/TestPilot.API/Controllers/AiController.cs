using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestPilot.Application.DTOs.AI;
using TestPilot.Application.Interfaces;

namespace TestPilot.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class AiController : ControllerBase
{
    private readonly IAiSuggestionService _aiService;

    public AiController(IAiSuggestionService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("suggest-tests")]
    public async Task<IActionResult> SuggestTests([FromBody] AiSuggestionRequestDto request)
    {
        var userId = GetCurrentUserId();
        var result = await _aiService.SuggestTestsAsync(request, userId);
        return Ok(result);
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
