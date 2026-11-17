using Microsoft.EntityFrameworkCore;
using TestPilot.Application.DTOs.Collections;
using TestPilot.Application.DTOs.Execution;
using TestPilot.Application.Interfaces;
using TestPilot.Domain.Entities;

namespace TestPilot.Application.Services;

public class ApiRequestService : IApiRequestService
{
    private readonly IApplicationDbContext _context;

    public ApiRequestService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiRequestDto> CreateRequestAsync(CreateApiRequestDto dto, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Request name is required.", nameof(dto.Name));

        if (string.IsNullOrWhiteSpace(dto.Url))
            throw new ArgumentException("Request URL is required.", nameof(dto.Url));

        // Verify collection ownership
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.CollectionId == dto.CollectionId && c.UserId == userId);

        if (collection == null)
        {
            throw new KeyNotFoundException($"Collection with ID '{dto.CollectionId}' was not found or access is denied.");
        }

        var apiRequest = new ApiRequest
        {
            RequestId = Guid.NewGuid(),
            CollectionId = dto.CollectionId,
            Name = dto.Name.Trim(),
            Url = dto.Url.Trim(),
            Method = dto.Method,
            Body = dto.Body,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Headers != null)
        {
            foreach (var header in dto.Headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)))
            {
                apiRequest.Headers.Add(new RequestHeader
                {
                    HeaderId = Guid.NewGuid(),
                    RequestId = apiRequest.RequestId,
                    HeaderKey = header.Key.Trim(),
                    HeaderValue = header.Value ?? string.Empty,
                    IsSensitive = header.IsSensitive
                });
            }
        }

        if (dto.QueryParams != null)
        {
            foreach (var param in dto.QueryParams.Where(p => !string.IsNullOrWhiteSpace(p.Key)))
            {
                apiRequest.QueryParams.Add(new QueryParameter
                {
                    ParameterId = Guid.NewGuid(),
                    RequestId = apiRequest.RequestId,
                    ParamKey = param.Key.Trim(),
                    ParamValue = param.Value ?? string.Empty
                });
            }
        }

        await _context.ApiRequests.AddAsync(apiRequest);
        await _context.SaveChangesAsync();

        return MapToDto(apiRequest);
    }

    public async Task<ApiRequestDto?> GetRequestByIdAsync(Guid requestId, Guid userId)
    {
        var request = await _context.ApiRequests
            .AsNoTracking()
            .Include(r => r.Collection)
            .Include(r => r.Headers)
            .Include(r => r.QueryParams)
            .FirstOrDefaultAsync(r => r.RequestId == requestId && r.Collection.UserId == userId);

        return request == null ? null : MapToDto(request);
    }

    public async Task<ApiRequestDto?> UpdateRequestAsync(Guid requestId, CreateApiRequestDto dto, Guid userId)
    {
        var request = await _context.ApiRequests
            .Include(r => r.Collection)
            .Include(r => r.Headers)
            .Include(r => r.QueryParams)
            .FirstOrDefaultAsync(r => r.RequestId == requestId && r.Collection.UserId == userId);

        if (request == null)
            return null;

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Request name is required.", nameof(dto.Name));

        if (string.IsNullOrWhiteSpace(dto.Url))
            throw new ArgumentException("Request URL is required.", nameof(dto.Url));

        request.Name = dto.Name.Trim();
        request.Url = dto.Url.Trim();
        request.Method = dto.Method;
        request.Body = dto.Body;

        // Replace headers
        request.Headers.Clear();
        if (dto.Headers != null)
        {
            foreach (var h in dto.Headers.Where(x => !string.IsNullOrWhiteSpace(x.Key)))
            {
                request.Headers.Add(new RequestHeader
                {
                    HeaderId = Guid.NewGuid(),
                    RequestId = request.RequestId,
                    HeaderKey = h.Key.Trim(),
                    HeaderValue = h.Value ?? string.Empty,
                    IsSensitive = h.IsSensitive
                });
            }
        }

        // Replace query parameters
        request.QueryParams.Clear();
        if (dto.QueryParams != null)
        {
            foreach (var p in dto.QueryParams.Where(x => !string.IsNullOrWhiteSpace(x.Key)))
            {
                request.QueryParams.Add(new QueryParameter
                {
                    ParameterId = Guid.NewGuid(),
                    RequestId = request.RequestId,
                    ParamKey = p.Key.Trim(),
                    ParamValue = p.Value ?? string.Empty
                });
            }
        }

        await _context.SaveChangesAsync();
        return MapToDto(request);
    }

    public async Task<bool> DeleteRequestAsync(Guid requestId, Guid userId)
    {
        var request = await _context.ApiRequests
            .Include(r => r.Collection)
            .FirstOrDefaultAsync(r => r.RequestId == requestId && r.Collection.UserId == userId);

        if (request == null)
            return false;

        _context.ApiRequests.Remove(request);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ApiRequestDto>> GetRequestsByCollectionAsync(Guid collectionId, Guid userId)
    {
        var collectionExists = await _context.Collections
            .AnyAsync(c => c.CollectionId == collectionId && c.UserId == userId);

        if (!collectionExists)
        {
            throw new KeyNotFoundException($"Collection with ID '{collectionId}' was not found or access is denied.");
        }

        var requests = await _context.ApiRequests
            .AsNoTracking()
            .Include(r => r.Headers)
            .Include(r => r.QueryParams)
            .Where(r => r.CollectionId == collectionId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return requests.Select(MapToDto);
    }

    private static ApiRequestDto MapToDto(ApiRequest r)
    {
        return new ApiRequestDto
        {
            RequestId = r.RequestId,
            CollectionId = r.CollectionId,
            Name = r.Name,
            Url = r.Url,
            Method = r.Method,
            Body = r.Body,
            CreatedAt = r.CreatedAt,
            Headers = r.Headers.Select(h => new KeyValuePairDto
            {
                Key = h.HeaderKey,
                Value = h.HeaderValue,
                IsSensitive = h.IsSensitive
            }).ToList(),
            QueryParams = r.QueryParams.Select(p => new KeyValuePairDto
            {
                Key = p.ParamKey,
                Value = p.ParamValue
            }).ToList()
        };
    }
}
