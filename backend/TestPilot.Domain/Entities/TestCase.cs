using TestPilot.Domain.Enums;

namespace TestPilot.Domain.Entities;

public class TestCase
{
    public Guid TestCaseId { get; set; } = Guid.NewGuid();
    public Guid RequestId { get; set; }
    public ApiRequest Request { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public AssertionType AssertionType { get; set; }
    public string ExpectedValue { get; set; } = string.Empty;
    public string? JsonPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
