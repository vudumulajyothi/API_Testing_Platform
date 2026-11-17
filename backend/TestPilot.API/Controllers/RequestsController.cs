using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestPilot.Application.DTOs.Collections;
using TestPilot.Application.Interfaces;

namespace TestPilot.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class RequestsController : ControllerBase
{
    private readonly IApiRequestService _requestService;

    public RequestsController(IApiRequestService requestService)
    {
        _requestService = requestService;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRequestById(Guid id)
    {
        var userId = GetCurrentUserId();
        var request = await _requestService.GetRequestByIdAsync(id, userId);

        if (request == null)
            return NotFound(new { message = $"Request '{id}' not found or access is denied." });

        return Ok(request);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateApiRequestDto dto)
    {
        var userId = GetCurrentUserId();
        var created = await _requestService.CreateRequestAsync(dto, userId);
        return CreatedAtAction(nameof(GetRequestById), new { id = created.RequestId }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRequest(Guid id, [FromBody] CreateApiRequestDto dto)
    {
        var userId = GetCurrentUserId();
        var updated = await _requestService.UpdateRequestAsync(id, dto, userId);

        if (updated == null)
            return NotFound(new { message = $"Request '{id}' not found or access is denied." });

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRequest(Guid id)
    {
        var userId = GetCurrentUserId();
        var deleted = await _requestService.DeleteRequestAsync(id, userId);

        if (!deleted)
            return NotFound(new { message = $"Request '{id}' not found or access is denied." });

        return NoContent();
    }

    [HttpGet("collection/{collectionId:guid}")]
    public async Task<IActionResult> GetRequestsByCollection(Guid collectionId)
    {
        var userId = GetCurrentUserId();
        var requests = await _requestService.GetRequestsByCollectionAsync(collectionId, userId);
        return Ok(requests);
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
