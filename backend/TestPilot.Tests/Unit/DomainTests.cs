using FluentAssertions;
using TestPilot.Domain.Entities;
using TestPilot.Domain.Enums;
using Xunit;

namespace TestPilot.Tests.Unit;

public class DomainTests
{
    [Fact]
    public void User_Initialization_ShouldSetDefaultsProperly()
    {
        // Act
        var user = new User
        {
            Name = "John Doe",
            Email = "john@example.com",
            PasswordHash = "hash123"
        };

        // Assert
        user.UserId.Should().NotBeEmpty();
        user.Name.Should().Be("John Doe");
        user.Email.Should().Be("john@example.com");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        user.Collections.Should().BeEmpty();
        user.RequestHistories.Should().BeEmpty();
    }

    [Fact]
    public void ApiRequest_Initialization_ShouldDefaultToGetMethod()
    {
        // Act
        var request = new ApiRequest
        {
            Name = "Fetch Products",
            Url = "https://api.example.com/products"
        };

        // Assert
        request.RequestId.Should().NotBeEmpty();
        request.Method.Should().Be(HttpMethodType.GET);
        request.Headers.Should().BeEmpty();
        request.QueryParams.Should().BeEmpty();
        request.TestCases.Should().BeEmpty();
    }

    [Fact]
    public void TestCase_ShouldStoreAssertionDetailsAccurately()
    {
        // Act
        var testCase = new TestCase
        {
            Name = "Status is 200",
            AssertionType = AssertionType.StatusCode,
            ExpectedValue = "200"
        };

        // Assert
        testCase.TestCaseId.Should().NotBeEmpty();
        testCase.AssertionType.Should().Be(AssertionType.StatusCode);
        testCase.ExpectedValue.Should().Be("200");
    }
}
