using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TestPilot.Application.DTOs.Auth;
using TestPilot.Application.Interfaces;
using TestPilot.Application.Services;
using TestPilot.Domain.Entities;
using TestPilot.Infrastructure.Data;
using TestPilot.Infrastructure.Services;
using Xunit;

namespace TestPilot.Tests.Unit;

public class AuthServiceTests
{
    private readonly TestPilotDbContext _dbContext;
    private readonly PasswordHasherService _hasher = new();
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock = new();
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestPilotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestPilotDbContext(options);
        _tokenGeneratorMock.Setup(t => t.GenerateToken(It.IsAny<User>()))
            .Returns("mocked-jwt-token-xyz");

        _authService = new AuthService(_dbContext, _hasher, _tokenGeneratorMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Name = "Alice Tester",
            Email = "alice@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Alice Tester");
        result.Email.Should().Be("alice@example.com");
        result.Token.Should().Be("mocked-jwt-token-xyz");

        var userInDb = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "alice@example.com");
        userInDb.Should().NotBeNull();
        userInDb!.PasswordHash.Should().NotBe("SecurePassword123!");
        _hasher.VerifyPassword("SecurePassword123!", userInDb.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Name = "Bob",
            Email = "duplicate@example.com",
            Password = "Password123!"
        };

        await _authService.RegisterAsync(request);

        var duplicateRequest = new RegisterRequestDto
        {
            Name = "Bob Clone",
            Email = "DUPLICATE@example.com", // testing case insensitivity
            Password = "OtherPassword123!"
        };

        // Act
        var act = async () => await _authService.RegisterAsync(duplicateRequest);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Name = "Charlie",
            Email = "charlie@example.com",
            PasswordHash = _hasher.HashPassword("CorrectPassword123!")
        };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequestDto
        {
            Email = "charlie@example.com",
            Password = "CorrectPassword123!"
        };

        // Act
        var response = await _authService.LoginAsync(loginRequest);

        // Assert
        response.Should().NotBeNull();
        response.Email.Should().Be("charlie@example.com");
        response.Token.Should().Be("mocked-jwt-token-xyz");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Name = "David",
            Email = "david@example.com",
            PasswordHash = _hasher.HashPassword("ActualPassword123!")
        };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var loginRequest = new LoginRequestDto
        {
            Email = "david@example.com",
            Password = "WrongPassword999!"
        };

        // Act
        var act = async () => await _authService.LoginAsync(loginRequest);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password.");
    }
}
