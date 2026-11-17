using TestPilot.Application.DTOs.AI;

namespace TestPilot.Application.Interfaces;

public interface IAiSuggestionService
{
    Task<AiSuggestionResultDto> SuggestTestsAsync(AiSuggestionRequestDto request, Guid userId);
}
