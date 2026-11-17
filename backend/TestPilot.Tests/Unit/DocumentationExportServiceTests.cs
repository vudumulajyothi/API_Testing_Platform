using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TestPilot.Application.Services;
using TestPilot.Domain.Entities;
using TestPilot.Domain.Enums;
using TestPilot.Infrastructure.Data;
using Xunit;

namespace TestPilot.Tests.Unit;

public class DocumentationExportServiceTests
{
    private readonly TestPilotDbContext _dbContext;
    private readonly DocumentationExportService _exportService;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _collectionId = Guid.NewGuid();

    public DocumentationExportServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestPilotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestPilotDbContext(options);
        _exportService = new DocumentationExportService(_dbContext);

        // Seed
        var user = new User { UserId = _userId, Name = "Owner", Email = "owner@test.com", PasswordHash = "h" };
        var collection = new Collection
        {
            CollectionId = _collectionId,
            UserId = _userId,
            Name = "Payment Gateway API",
            Description = "Payment processing and settlement endpoints"
        };
        var request = new ApiRequest
        {
            RequestId = Guid.NewGuid(),
            CollectionId = _collectionId,
            Name = "Authorize Charge",
            Url = "https://api.payments.com/v1/charge",
            Method = HttpMethodType.POST,
            Body = "{\"amount\": 100}"
        };
        request.Headers.Add(new RequestHeader { HeaderKey = "Authorization", HeaderValue = "Bearer secret-token", IsSensitive = true });
        request.QueryParams.Add(new QueryParameter { ParamKey = "currency", ParamValue = "USD" });

        _dbContext.Users.Add(user);
        _dbContext.Collections.Add(collection);
        _dbContext.ApiRequests.Add(request);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task ExportCollectionAsMarkdownAsync_ShouldGenerateValidMarkdownDocumentation()
    {
        // Act
        var markdown = await _exportService.ExportCollectionAsMarkdownAsync(_collectionId, _userId);

        // Assert
        markdown.Should().NotBeNullOrWhiteSpace();
        markdown.Should().Contain("# Payment Gateway API – API Documentation");
        markdown.Should().Contain("### `POST` Authorize Charge");
        markdown.Should().Contain("`https://api.payments.com/v1/charge`");
        markdown.Should().Contain("********"); // Sensitive header masked!
        markdown.Should().Contain("`currency`");
    }

    [Fact]
    public async Task ExportCollectionAsPostmanJsonAsync_ShouldGenerateValidPostmanJson()
    {
        // Act
        var json = await _exportService.ExportCollectionAsPostmanJsonAsync(_collectionId, _userId);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(json);
        var info = doc.RootElement.GetProperty("info");
        info.GetProperty("name").GetString().Should().Be("Payment Gateway API");
        doc.RootElement.GetProperty("item").GetArrayLength().Should().Be(1);
    }
}
