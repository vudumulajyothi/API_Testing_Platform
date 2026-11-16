using Microsoft.EntityFrameworkCore;
using TestPilot.Domain.Entities;

namespace TestPilot.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Collection> Collections { get; }
    DbSet<ApiRequest> ApiRequests { get; }
    DbSet<RequestHeader> RequestHeaders { get; }
    DbSet<QueryParameter> QueryParameters { get; }
    DbSet<TestCase> TestCases { get; }
    DbSet<TestRun> TestRuns { get; }
    DbSet<TestResult> TestResults { get; }
    DbSet<RequestHistory> RequestHistories { get; }
    DbSet<AiSuggestion> AiSuggestions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
