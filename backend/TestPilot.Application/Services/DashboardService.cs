using Microsoft.EntityFrameworkCore;
using TestPilot.Application.DTOs.Testing;
using TestPilot.Application.Interfaces;

namespace TestPilot.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;

    public DashboardService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId)
    {
        var totalCollections = await _context.Collections
            .CountAsync(c => c.UserId == userId);

        var userRequestIds = await _context.ApiRequests
            .Where(r => r.Collection.UserId == userId)
            .Select(r => r.RequestId)
            .ToListAsync();

        var totalRequests = userRequestIds.Count;

        var userTestRunsQuery = _context.TestRuns
            .Where(tr => userRequestIds.Contains(tr.RequestId));

        var totalTestRuns = await userTestRunsQuery.CountAsync();
        var passedTestRuns = await userTestRunsQuery.CountAsync(tr => tr.Passed);

        double passRate = totalTestRuns > 0
            ? Math.Round(((double)passedTestRuns / totalTestRuns) * 100, 1)
            : 100.0;

        var recentRuns = await userTestRunsQuery
            .AsNoTracking()
            .Include(tr => tr.Request)
            .OrderByDescending(tr => tr.ExecutedAt)
            .Take(5)
            .Select(tr => new TestRunSummaryDto
            {
                TestRunId = tr.TestRunId,
                RequestName = tr.Request.Name,
                Url = tr.Request.Url,
                StatusCode = tr.StatusCode,
                ResponseTimeMs = tr.ResponseTimeMs,
                Passed = tr.Passed,
                ExecutedAt = tr.ExecutedAt
            })
            .ToListAsync();

        return new DashboardSummaryDto
        {
            TotalRequests = totalRequests,
            TotalCollections = totalCollections,
            TotalTestRuns = totalTestRuns,
            PassRatePercentage = passRate,
            RecentRuns = recentRuns
        };
    }
}
