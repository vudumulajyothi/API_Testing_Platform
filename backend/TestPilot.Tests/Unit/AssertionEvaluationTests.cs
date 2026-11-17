using FluentAssertions;
using TestPilot.Application.DTOs.Execution;
using TestPilot.Application.Services;
using TestPilot.Domain.Entities;
using TestPilot.Domain.Enums;
using Xunit;

namespace TestPilot.Tests.Unit;

public class AssertionEvaluationTests
{
    private readonly AssertionEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_StatusCode_Matching_ShouldPass()
    {
        // Arrange
        var testCase = new TestCase
        {
            AssertionType = AssertionType.StatusCode,
            ExpectedValue = "200"
        };
        var response = new ExecuteResponseDto { StatusCode = 200 };

        // Act
        var result = _evaluator.Evaluate(testCase, response);

        // Assert
        result.Passed.Should().BeTrue();
        result.ActualValue.Should().Be("200");
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Evaluate_StatusCode_Mismatch_ShouldFailWithDescriptiveError()
    {
        // Arrange
        var testCase = new TestCase
        {
            AssertionType = AssertionType.StatusCode,
            ExpectedValue = "200"
        };
        var response = new ExecuteResponseDto { StatusCode = 404 };

        // Act
        var result = _evaluator.Evaluate(testCase, response);

        // Assert
        result.Passed.Should().BeFalse();
        result.ActualValue.Should().Be("404");
        result.ErrorMessage.Should().Contain("Expected status code '200', but received '404'");
    }

    [Fact]
    public void Evaluate_ResponseTime_UnderThreshold_ShouldPass()
    {
        // Arrange
        var testCase = new TestCase
        {
            AssertionType = AssertionType.ResponseTime,
            ExpectedValue = "< 500ms"
        };
        var response = new ExecuteResponseDto { ResponseTimeMs = 320 };

        // Act
        var result = _evaluator.Evaluate(testCase, response);

        // Assert
        result.Passed.Should().BeTrue();
        result.ActualValue.Should().Be("320ms");
    }

    [Fact]
    public void Evaluate_ResponseTime_OverThreshold_ShouldFail()
    {
        // Arrange
        var testCase = new TestCase
        {
            AssertionType = AssertionType.ResponseTime,
            ExpectedValue = "200"
        };
        var response = new ExecuteResponseDto { ResponseTimeMs = 450 };

        // Act
        var result = _evaluator.Evaluate(testCase, response);

        // Assert
        result.Passed.Should().BeFalse();
        result.ErrorMessage.Should().Contain("exceeded maximum threshold");
    }

    [Fact]
    public void Evaluate_JsonFieldEquals_MatchingPrimitive_ShouldPass()
    {
        // Arrange
        var testCase = new TestCase
        {
            AssertionType = AssertionType.JsonFieldEquals,
            JsonPath = "id",
            ExpectedValue = "1"
        };
        var response = new ExecuteResponseDto
        {
            RawBody = "{\"id\": 1, \"title\": \"Test Post\"}"
        };

        // Act
        var result = _evaluator.Evaluate(testCase, response);

        // Assert
        result.Passed.Should().BeTrue();
        result.ActualValue.Should().Be("1");
    }

    [Fact]
    public void Evaluate_JsonFieldEquals_NestedField_ShouldPass()
    {
        // Arrange
        var testCase = new TestCase
        {
            AssertionType = AssertionType.JsonFieldEquals,
            JsonPath = "user.profile.role",
            ExpectedValue = "Admin"
        };
        var response = new ExecuteResponseDto
        {
            RawBody = "{\"user\": {\"profile\": {\"role\": \"Admin\"}}}"
        };

        // Act
        var result = _evaluator.Evaluate(testCase, response);

        // Assert
        result.Passed.Should().BeTrue();
        result.ActualValue.Should().Be("Admin");
    }

    [Fact]
    public void Evaluate_JsonFieldExists_FieldPresent_ShouldPass()
    {
        // Arrange
        var testCase = new TestCase
        {
            AssertionType = AssertionType.JsonFieldExists,
            JsonPath = "data.token"
        };
        var response = new ExecuteResponseDto
        {
            RawBody = "{\"data\": {\"token\": \"abc-123\"}}"
        };

        // Act
        var result = _evaluator.Evaluate(testCase, response);

        // Assert
        result.Passed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_BodyContains_SubstringPresent_ShouldPass()
    {
        // Arrange
        var testCase = new TestCase
        {
            AssertionType = AssertionType.BodyContains,
            ExpectedValue = "success"
        };
        var response = new ExecuteResponseDto
        {
            RawBody = "{\"status\": \"SUCCESS\", \"message\": \"Operation completed\"}"
        };

        // Act
        var result = _evaluator.Evaluate(testCase, response);

        // Assert
        result.Passed.Should().BeTrue();
    }
}
