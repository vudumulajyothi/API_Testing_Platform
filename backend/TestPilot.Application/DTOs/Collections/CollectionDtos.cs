using TestPilot.Application.DTOs.Execution;
using TestPilot.Domain.Enums;

namespace TestPilot.Application.DTOs.Collections;

public class CollectionDto
{
    public Guid CollectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int RequestCount { get; set; }
}

public class CreateCollectionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ApiRequestDto
{
    public Guid RequestId { get; set; }
    public Guid CollectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public HttpMethodType Method { get; set; }
    public string? Body { get; set; }
    public List<KeyValuePairDto> Headers { get; set; } = new();
    public List<KeyValuePairDto> QueryParams { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CreateApiRequestDto
{
    public Guid CollectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public HttpMethodType Method { get; set; } = HttpMethodType.GET;
    public string? Body { get; set; }
    public List<KeyValuePairDto> Headers { get; set; } = new();
    public List<KeyValuePairDto> QueryParams { get; set; } = new();
}
