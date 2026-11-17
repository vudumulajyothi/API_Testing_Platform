using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestPilot.Application.DTOs.Collections;
using TestPilot.Application.Interfaces;

namespace TestPilot.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _collectionService;

    public CollectionsController(ICollectionService collectionService)
    {
        _collectionService = collectionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserCollections()
    {
        var userId = GetCurrentUserId();
        var collections = await _collectionService.GetUserCollectionsAsync(userId);
        return Ok(collections);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCollectionById(Guid id)
    {
        var userId = GetCurrentUserId();
        var collection = await _collectionService.GetCollectionByIdAsync(id, userId);

        if (collection == null)
            return NotFound(new { message = $"Collection '{id}' not found or access is denied." });

        return Ok(collection);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionDto dto)
    {
        var userId = GetCurrentUserId();
        var created = await _collectionService.CreateCollectionAsync(dto, userId);
        return CreatedAtAction(nameof(GetCollectionById), new { id = created.CollectionId }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCollection(Guid id, [FromBody] CreateCollectionDto dto)
    {
        var userId = GetCurrentUserId();
        var updated = await _collectionService.UpdateCollectionAsync(id, dto, userId);

        if (updated == null)
            return NotFound(new { message = $"Collection '{id}' not found or access is denied." });

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCollection(Guid id)
    {
        var userId = GetCurrentUserId();
        var deleted = await _collectionService.DeleteCollectionAsync(id, userId);

        if (!deleted)
            return NotFound(new { message = $"Collection '{id}' not found or access is denied." });

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
