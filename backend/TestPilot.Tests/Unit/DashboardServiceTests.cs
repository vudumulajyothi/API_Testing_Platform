using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TestPilot.Application.Services;
using TestPilot.Domain.Entities;
using TestPilot.Domain.Enums;
using TestPilot.Infrastructure.Data;
using Xunit;

namespace TestPilot.Tests.Unit;

public class DashboardServiceTests
{
    private readonly TestPilotDbContext _dbContext;
    private readonly DashboardService _dashboardService;
    private readonly Guid _userId = Guid.NewGuid();

    public DashboardServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestPilotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestPilotDbContext(options);
        _dashboardService = new DashboardService(_dbContext);

        // Seed user
        var user = new User { UserId = _userId, Name = "Owner", Email = "owner@test.com", PasswordHash = "h" };
        _dbContext.Users.Add(user);

        // Seed 2 collections
        var c1 = new Collection { CollectionId = Guid.NewGuid(), UserId = _userId, Name = "Users API" };
        var c2 = new Collection { CollectionId = Guid.NewGuid(), UserId = _userId, Name = "Orders API" };
        _dbContext.Collections.AddRange(c1, c2);

        // Seed 3 requests
        var r1 = new ApiRequest { RequestId = Guid.NewGuid(), CollectionId = c1.CollectionId, Name = "Get Users", Url = "https://api.com/users", Method = HttpMethodType.GET };
        var r2 = new ApiRequest { RequestId = Guid.NewGuid(), CollectionId = c1.CollectionId, Name = "Create User", Url = "https://api.com/users", Method = HttpMethodType.POST };
        var r3 = new ApiRequest { RequestId = Guid.NewGuid(), CollectionId = c2.CollectionId, Name = "Get Orders", Url = "https://api.com/orders", Method = HttpMethodType.GET };
        _dbContext.ApiRequests.AddRange(r1, r2, r3);

        // Seed 4 test runs: 3 passed, 1 failed (Pass rate: 75.0%)
        _dbContext.TestRuns.AddRange(
            new TestRun { TestRunId = Guid.NewGuid(), RequestId = r1.RequestId, StatusCode = 200, ResponseTimeMs = 120, Passed = true, ExecutedAt = DateTime.UtcNow.AddMinutes(-10) },
            new TestRun { TestRunId = Guid.NewGuid(), RequestId = r1.RequestId, StatusCode = 200, ResponseTimeMs = 110, Passed = true, ExecutedAt = DateTime.UtcNow.AddMinutes(-8) },
            new TestRun { TestRunId = Guid.NewGuid(), RequestId = r2.RequestId, StatusCode = 201, ResponseTimeMs = 210, Passed = true, ExecutedAt = DateTime.UtcNow.AddMinutes(-5) },
            new TestRun { TestRunId = Guid.NewGuid(), RequestId = r3.RequestId, StatusCode = 500, ResponseTimeMs = 90, Passed = false, ExecutedAt = DateTime.UtcNow.AddMinutes(-1) }
        );

        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldCalculateAggregatesAccurately()
    {
        // Act
        var summary = await _dashboardService.GetDashboardSummaryAsync(_userId);

        // Assert
        summary.Should().NotBeNull();
        summary.TotalCollections.Should().Be(2);
        summary.TotalRequests.Should().Be(3);
        summary.TotalTestRuns.Should().Be(4);
        summary.PassRatePercentage.Should().Be(75.0);
        summary.RecentRuns.Should().HaveCount(4);
        summary.RecentRuns.First().StatusCode.Should().Be(500); // most recent run
    }
}
