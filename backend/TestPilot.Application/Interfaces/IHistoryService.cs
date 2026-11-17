using TestPilot.Application.DTOs.History;

namespace TestPilot.Application.Interfaces;

public interface IHistoryService
{
    Task<PagedResultDto<RequestHistoryDto>> GetUserHistoryAsync(
        Guid userId, 
        int page = 1, 
        int pageSize = 20, 
        string? method = null, 
        int? statusCode = null);

    Task<bool> ClearUserHistoryAsync(Guid userId);
}
