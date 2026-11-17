using Microsoft.EntityFrameworkCore;
using TestPilot.Application.DTOs.History;
using TestPilot.Application.Interfaces;
using TestPilot.Domain.Enums;

namespace TestPilot.Application.Services;

public class HistoryService : IHistoryService
{
    private readonly IApplicationDbContext _context;

    public HistoryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<RequestHistoryDto>> GetUserHistoryAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 20, 
        string? method = null, 
        int? statusCode = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.RequestHistories
            .AsNoTracking()
            .Where(h => h.UserId == userId);

        if (!string.IsNullOrWhiteSpace(method) && Enum.TryParse<HttpMethodType>(method, true, out var parsedMethod))
        {
            query = query.Where(h => h.Method == parsedMethod);
        }

        if (statusCode.HasValue)
        {
            query = query.Where(h => h.StatusCode == statusCode.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(h => h.ExecutedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(h => new RequestHistoryDto
            {
                HistoryId = h.HistoryId,
                RequestId = h.RequestId,
                Method = h.Method,
                Url = h.Url,
                StatusCode = h.StatusCode,
                ResponseTimeMs = h.ResponseTimeMs,
                ResponseBody = h.ResponseBody,
                ExecutedAt = h.ExecutedAt
            })
            .ToListAsync();

        return new PagedResultDto<RequestHistoryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<bool> ClearUserHistoryAsync(Guid userId)
    {
        var records = await _context.RequestHistories
            .Where(h => h.UserId == userId)
            .ToListAsync();

        if (records.Count == 0)
            return false;

        _context.RequestHistories.RemoveRange(records);
        await _context.SaveChangesAsync();
        return true;
    }
}
