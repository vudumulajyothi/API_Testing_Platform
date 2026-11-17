using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestPilot.Application.DTOs.Testing;
using TestPilot.Application.Interfaces;

namespace TestPilot.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1")]
public class TestsController : ControllerBase
{
    private readonly ITestRunnerService _testRunnerService;

    public TestsController(ITestRunnerService testRunnerService)
    {
        _testRunnerService = testRunnerService;
    }

    [HttpGet("requests/{requestId:guid}/tests")]
    public async Task<IActionResult> GetTestCases(Guid requestId)
    {
        var userId = GetCurrentUserId();
        var testCases = await _testRunnerService.GetTestCasesByRequestIdAsync(requestId, userId);
        return Ok(testCases);
    }

    [HttpPost("requests/{requestId:guid}/tests")]
    public async Task<IActionResult> AddTestCase(Guid requestId, [FromBody] CreateTestCaseDto dto)
    {
        var userId = GetCurrentUserId();
        var created = await _testRunnerService.AddTestCaseAsync(requestId, dto, userId);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpDelete("requests/tests/{testCaseId:guid}")]
    public async Task<IActionResult> DeleteTestCase(Guid testCaseId)
    {
        var userId = GetCurrentUserId();
        var deleted = await _testRunnerService.DeleteTestCaseAsync(testCaseId, userId);

        if (!deleted)
            return NotFound(new { message = $"Test case '{testCaseId}' not found or access is denied." });

        return NoContent();
    }

    [HttpPost("requests/{requestId:guid}/run-tests")]
    public async Task<IActionResult> RunTests(Guid requestId)
    {
        var userId = GetCurrentUserId();
        var result = await _testRunnerService.RunTestsAsync(requestId, userId);
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
