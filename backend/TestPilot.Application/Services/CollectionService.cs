using Microsoft.EntityFrameworkCore;
using TestPilot.Application.DTOs.Collections;
using TestPilot.Application.Interfaces;
using TestPilot.Domain.Entities;

namespace TestPilot.Application.Services;

public class CollectionService : ICollectionService
{
    private readonly IApplicationDbContext _context;

    public CollectionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CollectionDto>> GetUserCollectionsAsync(Guid userId)
    {
        return await _context.Collections
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CollectionDto
            {
                CollectionId = c.CollectionId,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                RequestCount = c.Requests.Count
            })
            .ToListAsync();
    }

    public async Task<CollectionDto?> GetCollectionByIdAsync(Guid collectionId, Guid userId)
    {
        return await _context.Collections
            .AsNoTracking()
            .Where(c => c.CollectionId == collectionId && c.UserId == userId)
            .Select(c => new CollectionDto
            {
                CollectionId = c.CollectionId,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                RequestCount = c.Requests.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CollectionDto> CreateCollectionAsync(CreateCollectionDto dto, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Collection name is required.", nameof(dto.Name));

        var collection = new Collection
        {
            CollectionId = Guid.NewGuid(),
            UserId = userId,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Collections.AddAsync(collection);
        await _context.SaveChangesAsync();

        return new CollectionDto
        {
            CollectionId = collection.CollectionId,
            Name = collection.Name,
            Description = collection.Description,
            CreatedAt = collection.CreatedAt,
            RequestCount = 0
        };
    }

    public async Task<CollectionDto?> UpdateCollectionAsync(Guid collectionId, CreateCollectionDto dto, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Collection name is required.", nameof(dto.Name));

        var collection = await _context.Collections
            .Include(c => c.Requests)
            .FirstOrDefaultAsync(c => c.CollectionId == collectionId && c.UserId == userId);

        if (collection == null)
            return null;

        collection.Name = dto.Name.Trim();
        collection.Description = dto.Description?.Trim();

        await _context.SaveChangesAsync();

        return new CollectionDto
        {
            CollectionId = collection.CollectionId,
            Name = collection.Name,
            Description = collection.Description,
            CreatedAt = collection.CreatedAt,
            RequestCount = collection.Requests.Count
        };
    }

    public async Task<bool> DeleteCollectionAsync(Guid collectionId, Guid userId)
    {
        var collection = await _context.Collections
            .FirstOrDefaultAsync(c => c.CollectionId == collectionId && c.UserId == userId);

        if (collection == null)
            return false;

        _context.Collections.Remove(collection);
        await _context.SaveChangesAsync();
        return true;
    }
}
