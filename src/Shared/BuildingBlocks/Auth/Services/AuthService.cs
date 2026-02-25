using Auth.Exceptions;
using Auth.Models;
using Microsoft.Extensions.Logging;

namespace Auth.Services;

/// <summary>
/// Authentication service with in-memory mock users for demonstration.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// In-memory mock users for demo purposes.
    /// Passwords are BCrypt-hashed.
    /// </summary>
    private static readonly List<User> _users = new()
    {
        new User
        {
            Id = "user-001",
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            Roles = new List<string> { "User" }
        },
        new User
        {
            Id = "admin-001",
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Roles = new List<string> { "User", "Admin" }
        }
    };

    public AuthService(
        IJwtTokenGenerator tokenGenerator,
        JwtSettings jwtSettings,
        ILogger<AuthService> logger)
    {
        _tokenGenerator = tokenGenerator;
        _jwtSettings = jwtSettings;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TokenResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var user = await GetUserByEmailAsync(request.Email);

        if (user == null)
        {
            _logger.LogWarning("Login failed: user not found for email {Email}", request.Email);
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: invalid password for email {Email}", request.Email);
            throw new UnauthorizedException("Invalid email or password.");
        }

        var token = _tokenGenerator.GenerateToken(user.Id, user.Email, user.Roles);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        _logger.LogInformation("Login successful for user {UserId} ({Email})", user.Id, user.Email);

        return new TokenResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            TokenType = "Bearer"
        };
    }

    /// <inheritdoc />
    public Task<User?> GetUserByEmailAsync(string email)
    {
        var user = _users.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(user);
    }
}
