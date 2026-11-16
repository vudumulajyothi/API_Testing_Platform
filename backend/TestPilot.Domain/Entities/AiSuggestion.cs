namespace TestPilot.Domain.Entities;

public class AiSuggestion
{
    public Guid SuggestionId { get; set; } = Guid.NewGuid();
    public Guid RequestId { get; set; }
    public ApiRequest Request { get; set; } = null!;
    public string SuggestionText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
