using Microsoft.EntityFrameworkCore;
using TestPilot.Application.DTOs.Auth;
using TestPilot.Application.Interfaces;
using TestPilot.Domain.Entities;

namespace TestPilot.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasherService passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request.Name));

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            throw new ArgumentException("A valid email address is required.", nameof(request.Email));

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters long.", nameof(request.Password));

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail);

        if (existingUser)
        {
            throw new InvalidOperationException($"An account with email '{request.Email}' already exists.");
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Token = token
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Email and password are required.");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Token = token
        };
    }

    public async Task<AuthResponseDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return null;

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Token = token
        };
    }
}
