using TestPilot.Domain.Enums;

namespace TestPilot.Application.DTOs.Testing;

public class TestCaseDto
{
    public Guid TestCaseId { get; set; }
    public Guid RequestId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AssertionType AssertionType { get; set; }
    public string ExpectedValue { get; set; } = string.Empty;
    public string? JsonPath { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTestCaseDto
{
    public string Name { get; set; } = string.Empty;
    public AssertionType AssertionType { get; set; }
    public string ExpectedValue { get; set; } = string.Empty;
    public string? JsonPath { get; set; }
}

public class TestResultItemDto
{
    public Guid TestCaseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? ActualValue { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TestRunResultDto
{
    public Guid TestRunId { get; set; }
    public Guid RequestId { get; set; }
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public bool Passed { get; set; }
    public List<TestResultItemDto> Results { get; set; } = new();
}

public class DashboardSummaryDto
{
    public int TotalRequests { get; set; }
    public int TotalCollections { get; set; }
    public int TotalTestRuns { get; set; }
    public double PassRatePercentage { get; set; }
    public List<TestRunSummaryDto> RecentRuns { get; set; } = new();
}

public class TestRunSummaryDto
{
    public Guid TestRunId { get; set; }
    public string RequestName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public bool Passed { get; set; }
    public DateTime ExecutedAt { get; set; }
}
