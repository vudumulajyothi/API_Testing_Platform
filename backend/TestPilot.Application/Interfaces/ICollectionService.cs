using TestPilot.Application.DTOs.Collections;

namespace TestPilot.Application.Interfaces;

public interface ICollectionService
{
    Task<IEnumerable<CollectionDto>> GetUserCollectionsAsync(Guid userId);
    Task<CollectionDto?> GetCollectionByIdAsync(Guid collectionId, Guid userId);
    Task<CollectionDto> CreateCollectionAsync(CreateCollectionDto dto, Guid userId);
    Task<CollectionDto?> UpdateCollectionAsync(Guid collectionId, CreateCollectionDto dto, Guid userId);
    Task<bool> DeleteCollectionAsync(Guid collectionId, Guid userId);
}
