namespace TestPilot.Domain.Entities;

public class TestRun
{
    public Guid TestRunId { get; set; } = Guid.NewGuid();
    public Guid RequestId { get; set; }
    public ApiRequest Request { get; set; } = null!;
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public bool Passed { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
