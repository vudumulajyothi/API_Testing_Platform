using TestPilot.Application.DTOs.Testing;

namespace TestPilot.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId);
}
