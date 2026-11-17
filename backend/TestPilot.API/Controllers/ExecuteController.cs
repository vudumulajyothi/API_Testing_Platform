using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TestPilot.Application.DTOs.Execution;
using TestPilot.Application.Interfaces;

namespace TestPilot.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ExecuteController : ControllerBase
{
    private readonly IRequestExecutionService _executionService;

    public ExecuteController(IRequestExecutionService executionService)
    {
        _executionService = executionService;
    }

    [HttpPost]
    public async Task<IActionResult> Execute([FromBody] ExecuteRequestDto request)
    {
        Guid? userId = null;

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? User.FindFirstValue("sub");

        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedId))
        {
            userId = parsedId;
        }

        var response = await _executionService.ExecuteAsync(request, userId);
        return Ok(response);
    }
}
