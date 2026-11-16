namespace TestPilot.Domain.Entities;

public class TestResult
{
    public Guid ResultId { get; set; } = Guid.NewGuid();
    public Guid TestRunId { get; set; }
    public TestRun TestRun { get; set; } = null!;
    public Guid TestCaseId { get; set; }
    public TestCase TestCase { get; set; } = null!;
    public bool Passed { get; set; }
    public string? ActualValue { get; set; }
    public string? ErrorMessage { get; set; }
}
