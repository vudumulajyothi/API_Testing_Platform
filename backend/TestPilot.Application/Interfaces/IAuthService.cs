using TestPilot.Application.DTOs.Auth;

namespace TestPilot.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto?> GetCurrentUserAsync(Guid userId);
}
