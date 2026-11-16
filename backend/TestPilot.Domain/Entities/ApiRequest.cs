using TestPilot.Domain.Enums;

namespace TestPilot.Domain.Entities;

public class ApiRequest
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public Guid CollectionId { get; set; }
    public Collection Collection { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public HttpMethodType Method { get; set; } = HttpMethodType.GET;
    public string? Body { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RequestHeader> Headers { get; set; } = new List<RequestHeader>();
    public ICollection<QueryParameter> QueryParams { get; set; } = new List<QueryParameter>();
    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
    public ICollection<TestRun> TestRuns { get; set; } = new List<TestRun>();
    public ICollection<AiSuggestion> AiSuggestions { get; set; } = new List<AiSuggestion>();
}
