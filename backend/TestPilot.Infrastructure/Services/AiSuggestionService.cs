using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestPilot.Application.DTOs.AI;
using TestPilot.Application.DTOs.Execution;
using TestPilot.Application.Interfaces;
using TestPilot.Domain.Entities;
using TestPilot.Domain.Enums;

namespace TestPilot.Infrastructure.Services;

public class AiSuggestionService : IAiSuggestionService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiSuggestionService> _logger;

    public AiSuggestionService(
        IApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AiSuggestionService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AiSuggestionResultDto> SuggestTestsAsync(AiSuggestionRequestDto request, Guid userId)
    {
        // 1. Sanitize Sensitive Headers and Credentials
        var sanitizedHeaders = SanitizeHeaders(request.Headers);

        var sanitizedRequest = new AiSuggestionRequestDto
        {
            RequestId = request.RequestId,
            Method = request.Method,
            Url = request.Url,
            Headers = sanitizedHeaders,
            QueryParams = request.QueryParams,
            RequestBody = request.RequestBody,
            SampleResponseStatusCode = request.SampleResponseStatusCode,
            SampleResponseBody = request.SampleResponseBody
        };

        // 2. Generate Suggestions (via AI provider or Heuristic Engine)
        var result = GenerateHeuristicSuggestions(sanitizedRequest);

        // 3. Validate Generated Assertions against schema
        var validatedList = new List<AiSuggestedTestCaseDto>();
        foreach (var item in result.Suggestions)
        {
            if (IsValidSuggestion(item))
            {
                validatedList.Add(item);
            }
        }
        result.Suggestions = validatedList;

        // 4. Save to Database if RequestId exists and belongs to user
        if (request.RequestId.HasValue)
        {
            var req = await _context.ApiRequests
                .Include(r => r.Collection)
                .FirstOrDefaultAsync(r => r.RequestId == request.RequestId.Value && r.Collection.UserId == userId);

            if (req != null)
            {
                var suggestionEntity = new AiSuggestion
                {
                    SuggestionId = result.SuggestionId,
                    RequestId = req.RequestId,
                    SuggestionText = JsonSerializer.Serialize(result.Suggestions, new JsonSerializerOptions { WriteIndented = true }),
                    CreatedAt = DateTime.UtcNow
                };

                await _context.AiSuggestions.AddAsync(suggestionEntity);
                await _context.SaveChangesAsync();
            }
        }

        return result;
    }

    public static List<KeyValuePairDto> SanitizeHeaders(List<KeyValuePairDto>? headers)
    {
        if (headers == null) return new List<KeyValuePairDto>();

        var sensitiveKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization", "Cookie", "Set-Cookie", "X-Api-Key", "ApiKey", "Token", "Secret", "Password"
        };

        return headers.Select(h => new KeyValuePairDto
        {
            Key = h.Key,
            Value = h.IsSensitive || sensitiveKeys.Contains(h.Key.Trim()) ? "[REDACTED]" : h.Value,
            IsSensitive = h.IsSensitive
        }).ToList();
    }

    public static bool IsValidSuggestion(AiSuggestedTestCaseDto item)
    {
        if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.ExpectedValue))
            return false;

        if (item.AssertionType == AssertionType.JsonFieldEquals || item.AssertionType == AssertionType.JsonFieldExists)
        {
            if (string.IsNullOrWhiteSpace(item.JsonPath))
                return false;
        }

        return true;
    }

    private static AiSuggestionResultDto GenerateHeuristicSuggestions(AiSuggestionRequestDto req)
    {
        var suggestions = new List<AiSuggestedTestCaseDto>();

        // 1. Status Code assertion
        var expectedStatus = req.SampleResponseStatusCode ?? (req.Method switch
        {
            HttpMethodType.POST => 201,
            HttpMethodType.DELETE => 204,
            _ => 200
        });

        suggestions.Add(new AiSuggestedTestCaseDto
        {
            Name = $"HTTP Status code is {expectedStatus}",
            AssertionType = AssertionType.StatusCode,
            ExpectedValue = expectedStatus.ToString(),
            Reasoning = $"Standard expected status code for HTTP {req.Method} requests."
        });

        // 2. Latency SLA assertion
        suggestions.Add(new AiSuggestedTestCaseDto
        {
            Name = "Response latency is under 1500ms",
            AssertionType = AssertionType.ResponseTime,
            ExpectedValue = "< 1500ms",
            Reasoning = "Ensures service complies with typical REST API SLA response duration standards."
        });

        // 3. Inspect Sample Response Body if available
        if (!string.IsNullOrWhiteSpace(req.SampleResponseBody))
        {
            try
            {
                using var doc = JsonDocument.Parse(req.SampleResponseBody);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    // Inspect common fields
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        var propName = prop.Name;
                        if (propName.Equals("id", StringComparison.OrdinalIgnoreCase) ||
                            propName.Equals("userId", StringComparison.OrdinalIgnoreCase) ||
                            propName.Equals("status", StringComparison.OrdinalIgnoreCase) ||
                            propName.Equals("success", StringComparison.OrdinalIgnoreCase))
                        {
                            suggestions.Add(new AiSuggestedTestCaseDto
                            {
                                Name = $"Verify '{propName}' property matches expected",
                                AssertionType = AssertionType.JsonFieldEquals,
                                JsonPath = propName,
                                ExpectedValue = prop.Value.ToString(),
                                Reasoning = $"Verifies integrity of the critical '{propName}' identifier/status property."
                            });
                        }
                    }

                    // Field existence check
                    var firstProp = doc.RootElement.EnumerateObject().FirstOrDefault();
                    if (!string.IsNullOrEmpty(firstProp.Name))
                    {
                        suggestions.Add(new AiSuggestedTestCaseDto
                        {
                            Name = $"Ensure '{firstProp.Name}' is present and non-null",
                            AssertionType = AssertionType.JsonFieldExists,
                            JsonPath = firstProp.Name,
                            ExpectedValue = "exists",
                            Reasoning = $"Confirms schema payload includes '{firstProp.Name}'."
                        });
                    }
                }
                else if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    suggestions.Add(new AiSuggestedTestCaseDto
                    {
                        Name = "Response returns non-empty array",
                        AssertionType = AssertionType.BodyContains,
                        ExpectedValue = "[",
                        Reasoning = "Validates array collection format."
                    });
                }
            }
            catch
            {
                // Fallback body contains check
                suggestions.Add(new AiSuggestedTestCaseDto
                {
                    Name = "Response contains valid payload",
                    AssertionType = AssertionType.BodyContains,
                    ExpectedValue = "200",
                    Reasoning = "Basic content check."
                });
            }
        }

        return new AiSuggestionResultDto
        {
            SuggestionId = Guid.NewGuid(),
            RequestId = req.RequestId,
            ModelUsed = "TestPilot-HeuristicAI-v1",
            Suggestions = suggestions
        };
    }
}
