using TestPilot.Application.DTOs.Execution;

namespace TestPilot.Application.Interfaces;

public interface IRequestExecutionService
{
    Task<ExecuteResponseDto> ExecuteAsync(ExecuteRequestDto request, Guid? userId = null);
}
