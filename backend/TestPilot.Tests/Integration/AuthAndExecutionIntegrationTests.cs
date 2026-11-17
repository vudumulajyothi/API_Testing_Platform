using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TestPilot.Application.DTOs.Auth;
using TestPilot.Application.DTOs.Collections;
using Xunit;

namespace TestPilot.Tests.Integration;

public class AuthAndExecutionIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthAndExecutionIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturn200Healthy()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task CompleteUserLifecycle_Register_Login_AndAccessProtectedEndpoints()
    {
        var uniqueEmail = $"integration_{Guid.NewGuid():N}@testpilot.io";

        // 1. Register User
        var registerPayload = new RegisterRequestDto
        {
            Name = "Integration Tester",
            Email = uniqueEmail,
            Password = "SecurePassword123!"
        };

        var registerRes = await _client.PostAsJsonAsync("/api/v1/auth/register", registerPayload);
        registerRes.StatusCode.Should().Be(HttpStatusCode.Created);

        var registeredUser = await registerRes.Content.ReadFromJsonAsync<AuthResponseDto>();
        registeredUser.Should().NotBeNull();
        registeredUser!.Email.Should().Be(uniqueEmail);
        registeredUser.Token.Should().NotBeNullOrWhiteSpace();

        // 2. Login User
        var loginPayload = new LoginRequestDto
        {
            Email = uniqueEmail,
            Password = "SecurePassword123!"
        };

        var loginRes = await _client.PostAsJsonAsync("/api/v1/auth/login", loginPayload);
        loginRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var loggedInUser = await loginRes.Content.ReadFromJsonAsync<AuthResponseDto>();
        loggedInUser.Should().NotBeNull();
        loggedInUser!.Token.Should().NotBeNullOrWhiteSpace();

        // 3. Access Protected Route: /api/v1/auth/me
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loggedInUser.Token);
        var meRes = await _client.GetAsync("/api/v1/auth/me");
        meRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Create Collection as Authenticated User
        var collectionPayload = new CreateCollectionDto
        {
            Name = "Integration Suite",
            Description = "Automated integration testing collection"
        };

        var collRes = await _client.PostAsJsonAsync("/api/v1/collections", collectionPayload);
        collRes.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdColl = await collRes.Content.ReadFromJsonAsync<CollectionDto>();
        createdColl.Should().NotBeNull();
        createdColl!.Name.Should().Be("Integration Suite");

        // 5. Test Unauthorized Access Without Token
        _client.DefaultRequestHeaders.Authorization = null;
        var unauthRes = await _client.GetAsync("/api/v1/collections");
        unauthRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
