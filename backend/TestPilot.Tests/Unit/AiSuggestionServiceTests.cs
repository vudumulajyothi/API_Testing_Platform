using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using TestPilot.Application.DTOs.AI;
using TestPilot.Application.DTOs.Execution;
using TestPilot.Domain.Entities;
using TestPilot.Domain.Enums;
using TestPilot.Infrastructure.Data;
using TestPilot.Infrastructure.Services;
using Xunit;

namespace TestPilot.Tests.Unit;

public class AiSuggestionServiceTests
{
    private readonly TestPilotDbContext _dbContext;
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<ILogger<AiSuggestionService>> _loggerMock = new();
    private readonly AiSuggestionService _aiService;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _collectionId = Guid.NewGuid();
    private readonly Guid _requestId = Guid.NewGuid();

    public AiSuggestionServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestPilotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestPilotDbContext(options);

        // Seed
        _dbContext.Users.Add(new User { UserId = _userId, Name = "Dev", Email = "dev@test.com", PasswordHash = "h" });
        _dbContext.Collections.Add(new Collection { CollectionId = _collectionId, UserId = _userId, Name = "AI Col" });
        _dbContext.ApiRequests.Add(new ApiRequest
        {
            RequestId = _requestId,
            CollectionId = _collectionId,
            Name = "Get User Profile",
            Url = "https://api.example.com/user/10",
            Method = HttpMethodType.GET
        });
        _dbContext.SaveChanges();

        _aiService = new AiSuggestionService(_dbContext, _configMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void SanitizeHeaders_ShouldRedactSensitiveHeaders()
    {
        // Arrange
        var headers = new List<KeyValuePairDto>
        {
            new() { Key = "Authorization", Value = "Bearer top-secret-jwt" },
            new() { Key = "Cookie", Value = "session_id=12345" },
            new() { Key = "X-Api-Key", Value = "apikey_abc" },
            new() { Key = "Custom-Token", Value = "sensitive-val", IsSensitive = true },
            new() { Key = "Accept", Value = "application/json" }
        };

        // Act
        var sanitized = AiSuggestionService.SanitizeHeaders(headers);

        // Assert
        sanitized.First(h => h.Key == "Authorization").Value.Should().Be("[REDACTED]");
        sanitized.First(h => h.Key == "Cookie").Value.Should().Be("[REDACTED]");
        sanitized.First(h => h.Key == "X-Api-Key").Value.Should().Be("[REDACTED]");
        sanitized.First(h => h.Key == "Custom-Token").Value.Should().Be("[REDACTED]");
        sanitized.First(h => h.Key == "Accept").Value.Should().Be("application/json"); // Non-sensitive retained
    }

    [Fact]
    public void IsValidSuggestion_ShouldEnforceSchemaIntegrity()
    {
        // Valid
        AiSuggestionService.IsValidSuggestion(new AiSuggestedTestCaseDto
        {
            Name = "Status is 200",
            AssertionType = AssertionType.StatusCode,
            ExpectedValue = "200"
        }).Should().BeTrue();

        // Invalid: missing expected value
        AiSuggestionService.IsValidSuggestion(new AiSuggestedTestCaseDto
        {
            Name = "Status is 200",
            AssertionType = AssertionType.StatusCode,
            ExpectedValue = ""
        }).Should().BeFalse();

        // Invalid: JsonFieldEquals missing JsonPath
        AiSuggestionService.IsValidSuggestion(new AiSuggestedTestCaseDto
        {
            Name = "ID matches",
            AssertionType = AssertionType.JsonFieldEquals,
            ExpectedValue = "10",
            JsonPath = null
        }).Should().BeFalse();
    }

    [Fact]
    public async Task SuggestTestsAsync_WithSampleJson_ShouldGenerateAppropriateAssertions()
    {
        // Arrange
        var request = new AiSuggestionRequestDto
        {
            RequestId = _requestId,
            Method = HttpMethodType.GET,
            Url = "https://api.example.com/posts/1",
            SampleResponseStatusCode = 200,
            SampleResponseBody = "{\"id\": 1, \"userId\": 99, \"title\": \"Post Title\"}"
        };

        // Act
        var result = await _aiService.SuggestTestsAsync(request, _userId);

        // Assert
        result.Should().NotBeNull();
        result.Suggestions.Should().NotBeEmpty();

        // Verify status code assertion
        result.Suggestions.Should().Contain(s => s.AssertionType == AssertionType.StatusCode && s.ExpectedValue == "200");

        // Verify response time assertion
        result.Suggestions.Should().Contain(s => s.AssertionType == AssertionType.ResponseTime);

        // Verify JSON field assertion on 'id'
        result.Suggestions.Should().Contain(s => s.AssertionType == AssertionType.JsonFieldEquals && s.JsonPath == "id" && s.ExpectedValue == "1");

        // Verify database persistence in AiSuggestions table
        var inDb = await _dbContext.AiSuggestions.FirstOrDefaultAsync(s => s.RequestId == _requestId);
        inDb.Should().NotBeNull();
        inDb!.SuggestionText.Should().Contain("id");
    }
}
