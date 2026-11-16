using TestPilot.Application.DTOs.Collections;

namespace TestPilot.Application.Interfaces;

public interface IApiRequestService
{
    Task<ApiRequestDto?> GetRequestByIdAsync(Guid requestId, Guid userId);
    Task<ApiRequestDto> CreateRequestAsync(CreateApiRequestDto dto, Guid userId);
    Task<ApiRequestDto?> UpdateRequestAsync(Guid requestId, CreateApiRequestDto dto, Guid userId);
    Task<bool> DeleteRequestAsync(Guid requestId, Guid userId);
    Task<IEnumerable<ApiRequestDto>> GetRequestsByCollectionAsync(Guid collectionId, Guid userId);
}
