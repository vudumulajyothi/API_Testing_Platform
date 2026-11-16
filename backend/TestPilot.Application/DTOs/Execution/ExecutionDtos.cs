using TestPilot.Domain.Enums;

namespace TestPilot.Application.DTOs.Execution;

public class KeyValuePairDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsSensitive { get; set; } = false;
}

public class ExecuteRequestDto
{
    public HttpMethodType Method { get; set; } = HttpMethodType.GET;
    public string Url { get; set; } = string.Empty;
    public List<KeyValuePairDto> Headers { get; set; } = new();
    public List<KeyValuePairDto> QueryParams { get; set; } = new();
    public string? Body { get; set; }
    public Guid? RequestId { get; set; }
}

public class ExecuteResponseDto
{
    public int StatusCode { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public long ResponseTimeMs { get; set; }
    public long ContentLength { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public object? Body { get; set; }
    public string? RawBody { get; set; }
    public bool IsSuccessStatusCode { get; set; }
}
