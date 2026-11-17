using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TestPilot.Application.DTOs.Execution;
using TestPilot.Application.DTOs.Testing;
using TestPilot.Application.Interfaces;
using TestPilot.Application.Services;
using TestPilot.Domain.Entities;
using TestPilot.Domain.Enums;
using TestPilot.Infrastructure.Data;
using Xunit;

namespace TestPilot.Tests.Unit;

public class TestRunnerServiceTests
{
    private readonly TestPilotDbContext _dbContext;
    private readonly Mock<IRequestExecutionService> _executionMock = new();
    private readonly AssertionEvaluator _evaluator = new();
    private readonly TestRunnerService _testRunnerService;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();
    private readonly Guid _collectionId = Guid.NewGuid();
    private readonly Guid _requestId = Guid.NewGuid();

    public TestRunnerServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestPilotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestPilotDbContext(options);

        // Seed user, collection, and api request
        _dbContext.Users.AddRange(
            new User { UserId = _userId, Name = "Owner", Email = "owner@test.com", PasswordHash = "h" },
            new User { UserId = _otherUserId, Name = "Intruder", Email = "intruder@test.com", PasswordHash = "h" }
        );
        _dbContext.Collections.Add(
            new Collection { CollectionId = _collectionId, UserId = _userId, Name = "Suite" }
        );
        _dbContext.ApiRequests.Add(
            new ApiRequest
            {
                RequestId = _requestId,
                CollectionId = _collectionId,
                Name = "Get Post",
                Url = "https://jsonplaceholder.typicode.com/posts/1",
                Method = HttpMethodType.GET
            }
        );
        _dbContext.SaveChanges();

        _testRunnerService = new TestRunnerService(_dbContext, _executionMock.Object, _evaluator);
    }

    [Fact]
    public async Task AddTestCaseAsync_ValidInput_ShouldPersistTestCase()
    {
        // Arrange
        var dto = new CreateTestCaseDto
        {
            Name = "Status is 200",
            AssertionType = AssertionType.StatusCode,
            ExpectedValue = "200"
        };

        // Act
        var result = await _testRunnerService.AddTestCaseAsync(_requestId, dto, _userId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Status is 200");
        result.AssertionType.Should().Be(AssertionType.StatusCode);
        result.ExpectedValue.Should().Be("200");

        var inDb = await _dbContext.TestCases.FindAsync(result.TestCaseId);
        inDb.Should().NotBeNull();
    }

    [Fact]
    public async Task RunTestsAsync_AllPassingAssertions_ShouldRecordSuccessfulTestRun()
    {
        // Arrange
        await _testRunnerService.AddTestCaseAsync(_requestId, new CreateTestCaseDto
        {
            Name = "Status Code 200",
            AssertionType = AssertionType.StatusCode,
            ExpectedValue = "200"
        }, _userId);

        await _testRunnerService.AddTestCaseAsync(_requestId, new CreateTestCaseDto
        {
            Name = "Latency under 1s",
            AssertionType = AssertionType.ResponseTime,
            ExpectedValue = "< 1000ms"
        }, _userId);

        _executionMock.Setup(e => e.ExecuteAsync(It.IsAny<ExecuteRequestDto>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new ExecuteResponseDto
            {
                StatusCode = 200,
                ResponseTimeMs = 150,
                RawBody = "{\"id\": 1}"
            });

        // Act
        var result = await _testRunnerService.RunTestsAsync(_requestId, _userId);

        // Assert
        result.Should().NotBeNull();
        result.Passed.Should().BeTrue();
        result.Results.Should().HaveCount(2);
        result.Results.All(r => r.Passed).Should().BeTrue();

        var runInDb = await _dbContext.TestRuns
            .Include(tr => tr.TestResults)
            .FirstOrDefaultAsync(tr => tr.TestRunId == result.TestRunId);

        runInDb.Should().NotBeNull();
        runInDb!.Passed.Should().BeTrue();
        runInDb.TestResults.Should().HaveCount(2);
    }

    [Fact]
    public async Task RunTestsAsync_FailingAssertion_ShouldMarkRunAsFailed()
    {
        // Arrange
        await _testRunnerService.AddTestCaseAsync(_requestId, new CreateTestCaseDto
        {
            Name = "Expect 201 Created",
            AssertionType = AssertionType.StatusCode,
            ExpectedValue = "201"
        }, _userId);

        _executionMock.Setup(e => e.ExecuteAsync(It.IsAny<ExecuteRequestDto>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new ExecuteResponseDto
            {
                StatusCode = 200, // actual is 200, mismatch!
                ResponseTimeMs = 120
            });

        // Act
        var result = await _testRunnerService.RunTestsAsync(_requestId, _userId);

        // Assert
        result.Passed.Should().BeFalse();
        result.Results.First().Passed.Should().BeFalse();
        result.Results.First().ErrorMessage.Should().Contain("Expected status code '201'");
    }

    [Fact]
    public async Task RunTestsAsync_ByNonOwner_ShouldThrowKeyNotFoundException()
    {
        // Act
        var act = async () => await _testRunnerService.RunTestsAsync(_requestId, _otherUserId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
