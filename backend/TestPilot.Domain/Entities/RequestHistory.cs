using TestPilot.Domain.Enums;

namespace TestPilot.Domain.Entities;

public class RequestHistory
{
    public Guid HistoryId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid? RequestId { get; set; }
    public ApiRequest? Request { get; set; }
    public HttpMethodType Method { get; set; }
    public string Url { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public string? ResponseBody { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}
