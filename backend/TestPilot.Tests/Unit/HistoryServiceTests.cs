using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TestPilot.Application.Services;
using TestPilot.Domain.Entities;
using TestPilot.Domain.Enums;
using TestPilot.Infrastructure.Data;
using Xunit;

namespace TestPilot.Tests.Unit;

public class HistoryServiceTests
{
    private readonly TestPilotDbContext _dbContext;
    private readonly HistoryService _historyService;
    private readonly Guid _userA = Guid.NewGuid();
    private readonly Guid _userB = Guid.NewGuid();

    public HistoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestPilotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestPilotDbContext(options);
        _historyService = new HistoryService(_dbContext);

        // Seed users
        _dbContext.Users.AddRange(
            new User { UserId = _userA, Name = "User A", Email = "a@test.com", PasswordHash = "h" },
            new User { UserId = _userB, Name = "User B", Email = "b@test.com", PasswordHash = "h" }
        );

        // Seed history
        _dbContext.RequestHistories.AddRange(
            new RequestHistory { HistoryId = Guid.NewGuid(), UserId = _userA, Method = HttpMethodType.GET, Url = "https://api.com/1", StatusCode = 200, ResponseTimeMs = 120, ExecutedAt = DateTime.UtcNow.AddMinutes(-5) },
            new RequestHistory { HistoryId = Guid.NewGuid(), UserId = _userA, Method = HttpMethodType.POST, Url = "https://api.com/items", StatusCode = 201, ResponseTimeMs = 250, ExecutedAt = DateTime.UtcNow.AddMinutes(-4) },
            new RequestHistory { HistoryId = Guid.NewGuid(), UserId = _userA, Method = HttpMethodType.GET, Url = "https://api.com/2", StatusCode = 404, ResponseTimeMs = 80, ExecutedAt = DateTime.UtcNow.AddMinutes(-3) },
            new RequestHistory { HistoryId = Guid.NewGuid(), UserId = _userB, Method = HttpMethodType.GET, Url = "https://api.com/secret", StatusCode = 200, ResponseTimeMs = 50, ExecutedAt = DateTime.UtcNow }
        );
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetUserHistoryAsync_ShouldReturnOnlyUserRecordsWithPagination()
    {
        // Act
        var result = await _historyService.GetUserHistoryAsync(_userA, page: 1, pageSize: 2);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().NotContain(h => h.Url.Contains("secret")); // User B's record excluded
    }

    [Fact]
    public async Task GetUserHistoryAsync_WithMethodFilter_ShouldFilterProperly()
    {
        // Act
        var result = await _historyService.GetUserHistoryAsync(_userA, page: 1, pageSize: 10, method: "POST");

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.First().Method.Should().Be(HttpMethodType.POST);
        result.Items.First().StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task GetUserHistoryAsync_WithStatusCodeFilter_ShouldFilterProperly()
    {
        // Act
        var result = await _historyService.GetUserHistoryAsync(_userA, page: 1, pageSize: 10, statusCode: 404);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.First().StatusCode.Should().Be(404);
    }
}
