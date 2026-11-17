using System.Text.Json;
using System.Text.RegularExpressions;
using TestPilot.Application.DTOs.Execution;
using TestPilot.Application.Interfaces;
using TestPilot.Domain.Entities;
using TestPilot.Domain.Enums;

namespace TestPilot.Application.Services;

public class AssertionEvaluator : IAssertionEvaluator
{
    public AssertionEvaluationResult Evaluate(TestCase testCase, ExecuteResponseDto response)
    {
        return testCase.AssertionType switch
        {
            AssertionType.StatusCode => EvaluateStatusCode(testCase, response),
            AssertionType.ResponseTime => EvaluateResponseTime(testCase, response),
            AssertionType.JsonFieldEquals => EvaluateJsonFieldEquals(testCase, response),
            AssertionType.JsonFieldExists => EvaluateJsonFieldExists(testCase, response),
            AssertionType.BodyContains => EvaluateBodyContains(testCase, response),
            _ => new AssertionEvaluationResult
            {
                Passed = false,
                ActualValue = "Unknown assertion type",
                ErrorMessage = $"Assertion type '{testCase.AssertionType}' is not supported."
            }
        };
    }

    private static AssertionEvaluationResult EvaluateStatusCode(TestCase testCase, ExecuteResponseDto response)
    {
        var actual = response.StatusCode.ToString();
        var expected = testCase.ExpectedValue.Trim();

        var passed = string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);

        return new AssertionEvaluationResult
        {
            Passed = passed,
            ActualValue = actual,
            ErrorMessage = passed ? null : $"Expected status code '{expected}', but received '{actual}'."
        };
    }

    private static AssertionEvaluationResult EvaluateResponseTime(TestCase testCase, ExecuteResponseDto response)
    {
        var actual = response.ResponseTimeMs;
        var cleanExpected = Regex.Replace(testCase.ExpectedValue, @"[^\d]", "");

        if (!long.TryParse(cleanExpected, out var maxAllowedMs))
        {
            return new AssertionEvaluationResult
            {
                Passed = false,
                ActualValue = $"{actual}ms",
                ErrorMessage = $"Invalid threshold '{testCase.ExpectedValue}'. Must be numeric latency in ms."
            };
        }

        var passed = actual <= maxAllowedMs;

        return new AssertionEvaluationResult
        {
            Passed = passed,
            ActualValue = $"{actual}ms",
            ErrorMessage = passed ? null : $"Response time ({actual}ms) exceeded maximum threshold of {maxAllowedMs}ms."
        };
    }

    private static AssertionEvaluationResult EvaluateJsonFieldEquals(TestCase testCase, ExecuteResponseDto response)
    {
        if (string.IsNullOrWhiteSpace(testCase.JsonPath))
        {
            return new AssertionEvaluationResult
            {
                Passed = false,
                ActualValue = null,
                ErrorMessage = "JSON path cannot be empty for JsonFieldEquals assertion."
            };
        }

        if (string.IsNullOrWhiteSpace(response.RawBody))
        {
            return new AssertionEvaluationResult
            {
                Passed = false,
                ActualValue = null,
                ErrorMessage = "Response body was empty; cannot evaluate JSON path."
            };
        }

        try
        {
            using var doc = JsonDocument.Parse(response.RawBody);
            var element = ResolveJsonPath(doc.RootElement, testCase.JsonPath);

            if (element == null)
            {
                return new AssertionEvaluationResult
                {
                    Passed = false,
                    ActualValue = "null (field not found)",
                    ErrorMessage = $"Field at path '{testCase.JsonPath}' was not found in response JSON."
                };
            }

            var actualString = element.Value.ToString();
            var expectedString = testCase.ExpectedValue.Trim();

            var passed = string.Equals(actualString, expectedString, StringComparison.OrdinalIgnoreCase);

            return new AssertionEvaluationResult
            {
                Passed = passed,
                ActualValue = actualString,
                ErrorMessage = passed ? null : $"Field '{testCase.JsonPath}' had value '{actualString}', expected '{expectedString}'."
            };
        }
        catch (JsonException ex)
        {
            return new AssertionEvaluationResult
            {
                Passed = false,
                ActualValue = "Invalid JSON",
                ErrorMessage = $"Response was not valid JSON: {ex.Message}"
            };
        }
    }

    private static AssertionEvaluationResult EvaluateJsonFieldExists(TestCase testCase, ExecuteResponseDto response)
    {
        if (string.IsNullOrWhiteSpace(testCase.JsonPath))
        {
            return new AssertionEvaluationResult
            {
                Passed = false,
                ActualValue = null,
                ErrorMessage = "JSON path cannot be empty for JsonFieldExists assertion."
            };
        }

        if (string.IsNullOrWhiteSpace(response.RawBody))
        {
            return new AssertionEvaluationResult
            {
                Passed = false,
                ActualValue = null,
                ErrorMessage = "Response body was empty."
            };
        }

        try
        {
            using var doc = JsonDocument.Parse(response.RawBody);
            var element = ResolveJsonPath(doc.RootElement, testCase.JsonPath);

            var exists = element != null && element.Value.ValueKind != JsonValueKind.Null && element.Value.ValueKind != JsonValueKind.Undefined;

            return new AssertionEvaluationResult
            {
                Passed = exists,
                ActualValue = exists ? "exists" : "missing / null",
                ErrorMessage = exists ? null : $"Field at path '{testCase.JsonPath}' does not exist or is null."
            };
        }
        catch (JsonException ex)
        {
            return new AssertionEvaluationResult
            {
                Passed = false,
                ActualValue = "Invalid JSON",
                ErrorMessage = $"Response was not valid JSON: {ex.Message}"
            };
        }
    }

    private static AssertionEvaluationResult EvaluateBodyContains(TestCase testCase, ExecuteResponseDto response)
    {
        var rawBody = response.RawBody ?? string.Empty;
        var expected = testCase.ExpectedValue.Trim();

        var passed = rawBody.Contains(expected, StringComparison.OrdinalIgnoreCase);

        return new AssertionEvaluationResult
        {
            Passed = passed,
            ActualValue = passed ? $"Contains '{expected}'" : "Substring not found",
            ErrorMessage = passed ? null : $"Response body does not contain expected substring '{expected}'."
        };
    }

    private static JsonElement? ResolveJsonPath(JsonElement root, string path)
    {
        var parts = path.Trim().Split(new[] { '.', '/', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
        var current = root;

        foreach (var part in parts)
        {
            if (int.TryParse(part, out var arrayIndex))
            {
                if (current.ValueKind != JsonValueKind.Array || arrayIndex < 0 || arrayIndex >= current.GetArrayLength())
                    return null;

                current = current[arrayIndex];
            }
            else
            {
                if (current.ValueKind != JsonValueKind.Object)
                    return null;

                var matched = false;
                foreach (var prop in current.EnumerateObject())
                {
                    if (prop.Name.Equals(part, StringComparison.OrdinalIgnoreCase))
                    {
                        current = prop.Value;
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                    return null;
            }
        }

        return current;
    }
}
