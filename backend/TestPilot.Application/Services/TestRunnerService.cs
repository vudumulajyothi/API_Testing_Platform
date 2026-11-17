using Microsoft.EntityFrameworkCore;
using TestPilot.Application.DTOs.Collections;
using TestPilot.Application.DTOs.Execution;
using TestPilot.Application.DTOs.Testing;
using TestPilot.Application.Interfaces;
using TestPilot.Domain.Entities;

namespace TestPilot.Application.Services;

public class TestRunnerService : ITestRunnerService
{
    private readonly IApplicationDbContext _context;
    private readonly IRequestExecutionService _executionService;
    private readonly IAssertionEvaluator _assertionEvaluator;

    public TestRunnerService(
        IApplicationDbContext context,
        IRequestExecutionService executionService,
        IAssertionEvaluator assertionEvaluator)
    {
        _context = context;
        _executionService = executionService;
        _assertionEvaluator = assertionEvaluator;
    }

    public async Task<IEnumerable<TestCaseDto>> GetTestCasesByRequestIdAsync(Guid requestId, Guid userId)
    {
        var request = await _context.ApiRequests
            .AsNoTracking()
            .Include(r => r.Collection)
            .Include(r => r.TestCases)
            .FirstOrDefaultAsync(r => r.RequestId == requestId && r.Collection.UserId == userId);

        if (request == null)
        {
            throw new KeyNotFoundException($"Request '{requestId}' was not found or access is denied.");
        }

        return request.TestCases
            .OrderBy(t => t.CreatedAt)
            .Select(t => new TestCaseDto
            {
                TestCaseId = t.TestCaseId,
                RequestId = t.RequestId,
                Name = t.Name,
                AssertionType = t.AssertionType,
                ExpectedValue = t.ExpectedValue,
                JsonPath = t.JsonPath,
                CreatedAt = t.CreatedAt
            });
    }

    public async Task<TestCaseDto> AddTestCaseAsync(Guid requestId, CreateTestCaseDto dto, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Assertion name is required.", nameof(dto.Name));

        if (string.IsNullOrWhiteSpace(dto.ExpectedValue))
            throw new ArgumentException("Expected value is required.", nameof(dto.ExpectedValue));

        var request = await _context.ApiRequests
            .Include(r => r.Collection)
            .FirstOrDefaultAsync(r => r.RequestId == requestId && r.Collection.UserId == userId);

        if (request == null)
        {
            throw new KeyNotFoundException($"Request '{requestId}' was not found or access is denied.");
        }

        var testCase = new TestCase
        {
            TestCaseId = Guid.NewGuid(),
            RequestId = requestId,
            Name = dto.Name.Trim(),
            AssertionType = dto.AssertionType,
            ExpectedValue = dto.ExpectedValue.Trim(),
            JsonPath = dto.JsonPath?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _context.TestCases.AddAsync(testCase);
        await _context.SaveChangesAsync();

        return new TestCaseDto
        {
            TestCaseId = testCase.TestCaseId,
            RequestId = testCase.RequestId,
            Name = testCase.Name,
            AssertionType = testCase.AssertionType,
            ExpectedValue = testCase.ExpectedValue,
            JsonPath = testCase.JsonPath,
            CreatedAt = testCase.CreatedAt
        };
    }

    public async Task<bool> DeleteTestCaseAsync(Guid testCaseId, Guid userId)
    {
        var testCase = await _context.TestCases
            .Include(t => t.Request)
            .ThenInclude(r => r.Collection)
            .FirstOrDefaultAsync(t => t.TestCaseId == testCaseId && t.Request.Collection.UserId == userId);

        if (testCase == null)
            return false;

        _context.TestCases.Remove(testCase);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TestRunResultDto> RunTestsAsync(Guid requestId, Guid userId)
    {
        var request = await _context.ApiRequests
            .Include(r => r.Collection)
            .Include(r => r.Headers)
            .Include(r => r.QueryParams)
            .Include(r => r.TestCases)
            .FirstOrDefaultAsync(r => r.RequestId == requestId && r.Collection.UserId == userId);

        if (request == null)
        {
            throw new KeyNotFoundException($"Request '{requestId}' was not found or access is denied.");
        }

        if (request.TestCases.Count == 0)
        {
            throw new InvalidOperationException("No test assertions are configured for this request.");
        }

        // 1. Prepare and execute the request
        var executePayload = new ExecuteRequestDto
        {
            RequestId = request.RequestId,
            Method = request.Method,
            Url = request.Url,
            Body = request.Body,
            Headers = request.Headers.Select(h => new KeyValuePairDto
            {
                Key = h.HeaderKey,
                Value = h.HeaderValue,
                IsSensitive = h.IsSensitive
            }).ToList(),
            QueryParams = request.QueryParams.Select(p => new KeyValuePairDto
            {
                Key = p.ParamKey,
                Value = p.ParamValue
            }).ToList()
        };

        var executionResponse = await _executionService.ExecuteAsync(executePayload, userId);

        // 2. Evaluate all configured assertions
        var results = new List<TestResultItemDto>();
        var testRun = new TestRun
        {
            TestRunId = Guid.NewGuid(),
            RequestId = requestId,
            StatusCode = executionResponse.StatusCode,
            ResponseTimeMs = executionResponse.ResponseTimeMs,
            ExecutedAt = DateTime.UtcNow
        };

        foreach (var testCase in request.TestCases)
        {
            var eval = _assertionEvaluator.Evaluate(testCase, executionResponse);

            var resultItem = new TestResultItemDto
            {
                TestCaseId = testCase.TestCaseId,
                Name = testCase.Name,
                Passed = eval.Passed,
                ActualValue = eval.ActualValue,
                ErrorMessage = eval.ErrorMessage
            };

            results.Add(resultItem);

            testRun.TestResults.Add(new TestResult
            {
                ResultId = Guid.NewGuid(),
                TestRunId = testRun.TestRunId,
                TestCaseId = testCase.TestCaseId,
                Passed = eval.Passed,
                ActualValue = eval.ActualValue,
                ErrorMessage = eval.ErrorMessage
            });
        }

        testRun.Passed = results.All(r => r.Passed);

        await _context.TestRuns.AddAsync(testRun);
        await _context.SaveChangesAsync();

        return new TestRunResultDto
        {
            TestRunId = testRun.TestRunId,
            RequestId = requestId,
            StatusCode = executionResponse.StatusCode,
            ResponseTimeMs = executionResponse.ResponseTimeMs,
            Passed = testRun.Passed,
            Results = results
        };
    }
}
