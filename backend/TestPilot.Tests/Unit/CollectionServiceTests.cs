using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TestPilot.Application.DTOs.Collections;
using TestPilot.Application.Services;
using TestPilot.Domain.Entities;
using TestPilot.Infrastructure.Data;
using Xunit;

namespace TestPilot.Tests.Unit;

public class CollectionServiceTests
{
    private readonly TestPilotDbContext _dbContext;
    private readonly CollectionService _collectionService;
    private readonly Guid _userA = Guid.NewGuid();
    private readonly Guid _userB = Guid.NewGuid();

    public CollectionServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestPilotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestPilotDbContext(options);
        _collectionService = new CollectionService(_dbContext);

        // Seed users
        _dbContext.Users.AddRange(
            new User { UserId = _userA, Name = "User A", Email = "a@test.com", PasswordHash = "hashA" },
            new User { UserId = _userB, Name = "User B", Email = "b@test.com", PasswordHash = "hashB" }
        );
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task CreateCollectionAsync_ShouldPersistAndReturnDto()
    {
        // Arrange
        var dto = new CreateCollectionDto
        {
            Name = "Payment Endpoints",
            Description = "Endpoints handling checkout"
        };

        // Act
        var result = await _collectionService.CreateCollectionAsync(dto, _userA);

        // Assert
        result.Should().NotBeNull();
        result.CollectionId.Should().NotBeEmpty();
        result.Name.Should().Be("Payment Endpoints");
        result.Description.Should().Be("Endpoints handling checkout");
        result.RequestCount.Should().Be(0);

        var saved = await _dbContext.Collections.FirstOrDefaultAsync(c => c.CollectionId == result.CollectionId);
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(_userA);
    }

    [Fact]
    public async Task GetUserCollectionsAsync_ShouldOnlyReturnOwnCollections()
    {
        // Arrange
        await _collectionService.CreateCollectionAsync(new CreateCollectionDto { Name = "A's Collection 1" }, _userA);
        await _collectionService.CreateCollectionAsync(new CreateCollectionDto { Name = "A's Collection 2" }, _userA);
        await _collectionService.CreateCollectionAsync(new CreateCollectionDto { Name = "B's Collection 1" }, _userB);

        // Act
        var userACollections = await _collectionService.GetUserCollectionsAsync(_userA);
        var userBCollections = await _collectionService.GetUserCollectionsAsync(_userB);

        // Assert
        userACollections.Should().HaveCount(2);
        userACollections.Select(c => c.Name).Should().Contain(new[] { "A's Collection 1", "A's Collection 2" });
        userACollections.Select(c => c.Name).Should().NotContain("B's Collection 1");

        userBCollections.Should().HaveCount(1);
        userBCollections.First().Name.Should().Be("B's Collection 1");
    }

    [Fact]
    public async Task UpdateCollectionAsync_ByNonOwner_ShouldReturnNull()
    {
        // Arrange
        var created = await _collectionService.CreateCollectionAsync(new CreateCollectionDto { Name = "Owner is A" }, _userA);

        // Act
        var result = await _collectionService.UpdateCollectionAsync(
            created.CollectionId,
            new CreateCollectionDto { Name = "Attempted hijack by B" },
            _userB
        );

        // Assert
        result.Should().BeNull();

        var inDb = await _dbContext.Collections.FindAsync(created.CollectionId);
        inDb!.Name.Should().Be("Owner is A");
    }

    [Fact]
    public async Task DeleteCollectionAsync_ByNonOwner_ShouldReturnFalseAndNotDelete()
    {
        // Arrange
        var created = await _collectionService.CreateCollectionAsync(new CreateCollectionDto { Name = "Delete Test" }, _userA);

        // Act
        var deleteResult = await _collectionService.DeleteCollectionAsync(created.CollectionId, _userB);

        // Assert
        deleteResult.Should().BeFalse();
        (await _dbContext.Collections.AnyAsync(c => c.CollectionId == created.CollectionId)).Should().BeTrue();
    }
}
