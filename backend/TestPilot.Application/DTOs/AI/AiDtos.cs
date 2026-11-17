using TestPilot.Application.DTOs.Execution;
using TestPilot.Domain.Enums;

namespace TestPilot.Application.DTOs.AI;

public class AiSuggestionRequestDto
{
    public Guid? RequestId { get; set; }
    public HttpMethodType Method { get; set; } = HttpMethodType.GET;
    public string Url { get; set; } = string.Empty;
    public List<KeyValuePairDto> Headers { get; set; } = new();
    public List<KeyValuePairDto> QueryParams { get; set; } = new();
    public string? RequestBody { get; set; }
    public int? SampleResponseStatusCode { get; set; }
    public string? SampleResponseBody { get; set; }
}

public class AiSuggestedTestCaseDto
{
    public string Name { get; set; } = string.Empty;
    public AssertionType AssertionType { get; set; }
    public string ExpectedValue { get; set; } = string.Empty;
    public string? JsonPath { get; set; }
    public string? Reasoning { get; set; }
}

public class AiSuggestionResultDto
{
    public Guid SuggestionId { get; set; }
    public Guid? RequestId { get; set; }
    public string ModelUsed { get; set; } = string.Empty;
    public List<AiSuggestedTestCaseDto> Suggestions { get; set; } = new();
}
