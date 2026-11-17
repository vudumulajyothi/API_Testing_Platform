using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestPilot.Application.Interfaces;

namespace TestPilot.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/collections/{id:guid}/export")]
public class DocumentationController : ControllerBase
{
    private readonly IDocumentationExportService _exportService;

    public DocumentationController(IDocumentationExportService exportService)
    {
        _exportService = exportService;
    }

    [HttpGet("markdown")]
    public async Task<IActionResult> ExportMarkdown(Guid id)
    {
        var userId = GetCurrentUserId();
        var markdown = await _exportService.ExportCollectionAsMarkdownAsync(id, userId);

        var bytes = Encoding.UTF8.GetBytes(markdown);
        return File(bytes, "text/markdown", $"collection-{id}-docs.md");
    }

    [HttpGet("postman")]
    public async Task<IActionResult> ExportPostman(Guid id)
    {
        var userId = GetCurrentUserId();
        var json = await _exportService.ExportCollectionAsPostmanJsonAsync(id, userId);

        var bytes = Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"collection-{id}.postman_collection.json");
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
