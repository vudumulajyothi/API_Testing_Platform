namespace TestPilot.Domain.Entities;

public class RequestHeader
{
    public Guid HeaderId { get; set; } = Guid.NewGuid();
    public Guid RequestId { get; set; }
    public ApiRequest Request { get; set; } = null!;
    public string HeaderKey { get; set; } = string.Empty;
    public string HeaderValue { get; set; } = string.Empty;
    public bool IsSensitive { get; set; } = false;
}
