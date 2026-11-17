using FluentAssertions;
using TestPilot.Infrastructure.Services;
using Xunit;

namespace TestPilot.Tests.Unit;

public class PasswordHasherServiceTests
{
    private readonly PasswordHasherService _hasher = new();

    [Fact]
    public void HashPassword_ShouldReturnFormattedString()
    {
        // Act
        var hash = _hasher.HashPassword("SecurePassword123!");

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Split(':').Should().HaveCount(3);
    }

    [Fact]
    public void HashPassword_SamePasswordHashedTwice_ShouldProduceDifferentSalts()
    {
        // Act
        var hash1 = _hasher.HashPassword("TestPassword");
        var hash2 = _hasher.HashPassword("TestPassword");

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        const string password = "MyP@ssw0rd!2026";
        var hash = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var hash = _hasher.HashPassword("CorrectPassword");

        // Act
        var result = _hasher.VerifyPassword("WrongPassword", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void VerifyPassword_WithInvalidInputs_ShouldReturnFalse(string? input)
    {
        // Arrange
        var hash = _hasher.HashPassword("ValidPassword");

        // Act
        var result = _hasher.VerifyPassword(input!, hash);

        // Assert
        result.Should().BeFalse();
    }
}
