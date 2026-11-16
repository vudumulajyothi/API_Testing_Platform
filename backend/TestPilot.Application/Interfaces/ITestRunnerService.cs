using TestPilot.Application.DTOs.Testing;

namespace TestPilot.Application.Interfaces;

public interface ITestRunnerService
{
    Task<IEnumerable<TestCaseDto>> GetTestCasesByRequestIdAsync(Guid requestId, Guid userId);
    Task<TestCaseDto> AddTestCaseAsync(Guid requestId, CreateTestCaseDto dto, Guid userId);
    Task<bool> DeleteTestCaseAsync(Guid testCaseId, Guid userId);
    Task<TestRunResultDto> RunTestsAsync(Guid requestId, Guid userId);
}
