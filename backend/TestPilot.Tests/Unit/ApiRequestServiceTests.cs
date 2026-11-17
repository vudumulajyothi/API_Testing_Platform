using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TestPilot.Application.DTOs.Collections;
using TestPilot.Application.DTOs.Execution;
using TestPilot.Application.Services;
using TestPilot.Domain.Entities;
using TestPilot.Domain.Enums;
using TestPilot.Infrastructure.Data;
using Xunit;

namespace TestPilot.Tests.Unit;

public class ApiRequestServiceTests
{
    private readonly TestPilotDbContext _dbContext;
    private readonly ApiRequestService _requestService;
    private readonly Guid _userA = Guid.NewGuid();
    private readonly Guid _userB = Guid.NewGuid();
    private readonly Guid _collectionA = Guid.NewGuid();

    public ApiRequestServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestPilotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestPilotDbContext(options);
        _requestService = new ApiRequestService(_dbContext);

        // Seed users & collection
        _dbContext.Users.AddRange(
            new User { UserId = _userA, Name = "User A", Email = "a@test.com", PasswordHash = "hash" },
            new User { UserId = _userB, Name = "User B", Email = "b@test.com", PasswordHash = "hash" }
        );
        _dbContext.Collections.Add(
            new Collection { CollectionId = _collectionA, UserId = _userA, Name = "User A Collection" }
        );
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task CreateRequestAsync_ValidInput_ShouldPersistWithHeadersAndParams()
    {
        // Arrange
        var dto = new CreateApiRequestDto
        {
            CollectionId = _collectionA,
            Name = "Get User Profile",
            Url = "https://api.example.com/users/42",
            Method = HttpMethodType.GET,
            Headers = new List<KeyValuePairDto>
            {
                new() { Key = "Authorization", Value = "Bearer abc", IsSensitive = true },
                new() { Key = "Accept", Value = "application/json" }
            },
            QueryParams = new List<KeyValuePairDto>
            {
                new() { Key = "includePosts", Value = "true" }
            }
        };

        // Act
        var result = await _requestService.CreateRequestAsync(dto, _userA);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Get User Profile");
        result.Headers.Should().HaveCount(2);
        result.QueryParams.Should().HaveCount(1);

        var inDb = await _dbContext.ApiRequests
            .Include(r => r.Headers)
            .Include(r => r.QueryParams)
            .FirstOrDefaultAsync(r => r.RequestId == result.RequestId);

        inDb.Should().NotBeNull();
        inDb!.Headers.Should().HaveCount(2);
        inDb.Headers.First(h => h.HeaderKey == "Authorization").IsSensitive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateRequestAsync_ToAnotherUsersCollection_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var dto = new CreateApiRequestDto
        {
            CollectionId = _collectionA, // belongs to userA
            Name = "Unauthorized Request Creation",
            Url = "https://example.com"
        };

        // Act
        var act = async () => await _requestService.CreateRequestAsync(dto, _userB); // userB attempting

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetRequestByIdAsync_NonOwner_ShouldReturnNull()
    {
        // Arrange
        var created = await _requestService.CreateRequestAsync(new CreateApiRequestDto
        {
            CollectionId = _collectionA,
            Name = "Private Request",
            Url = "https://api.example.com/secret"
        }, _userA);

        // Act
        var result = await _requestService.GetRequestByIdAsync(created.RequestId, _userB);

        // Assert
        result.Should().BeNull();
    }
}
