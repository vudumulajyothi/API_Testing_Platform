namespace TestPilot.Domain.Entities;

public class Collection
{
    public Guid CollectionId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ApiRequest> Requests { get; set; } = new List<ApiRequest>();
}
