using TestPilot.Application.DTOs.Execution;
using TestPilot.Domain.Entities;

namespace TestPilot.Application.Interfaces;

public class AssertionEvaluationResult
{
    public bool Passed { get; set; }
    public string? ActualValue { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IAssertionEvaluator
{
    AssertionEvaluationResult Evaluate(TestCase testCase, ExecuteResponseDto response);
}
