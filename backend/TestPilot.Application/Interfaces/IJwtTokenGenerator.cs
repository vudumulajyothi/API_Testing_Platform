using TestPilot.Domain.Entities;

namespace TestPilot.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
