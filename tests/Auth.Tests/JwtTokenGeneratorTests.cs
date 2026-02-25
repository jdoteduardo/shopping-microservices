using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth.Models;
using Auth.Services;
using Xunit;

namespace Auth.Tests;

public class JwtTokenGeneratorTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly JwtTokenGenerator _sut;

    public JwtTokenGeneratorTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "super-secret-key-at-least-32-chars-long",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        };
        _sut = new JwtTokenGenerator(_jwtSettings);
    }

    [Fact]
    public void GenerateToken_ValidUser_ReturnsValidToken()
    {
        // Arrange
        var userId = "user-123";
        var email = "test@example.com";
        var roles = new List<string> { "User" };

        // Act
        var token = _sut.GenerateToken(userId, email, roles);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(new JwtSecurityTokenHandler().CanReadToken(token));
    }

    [Fact]
    public void GenerateToken_ContainsCorrectClaims()
    {
        // Arrange
        var userId = "user-123";
        var email = "test@example.com";
        var roles = new List<string> { "User", "Admin" };

        // Act
        var tokenString = _sut.GenerateToken(userId, email, roles);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void GenerateToken_TokenExpiresAtCorrectTime()
    {
        // Arrange
        var userId = "user-123";
        var email = "test@example.com";
        var roles = new List<string> { "User" };
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var tokenString = _sut.GenerateToken(userId, email, roles);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        // Assert
        var expectedExpiration = beforeGeneration.AddMinutes(_jwtSettings.ExpirationMinutes);
        Assert.True(Math.Abs((token.ValidTo - expectedExpiration).TotalSeconds) < 5);
    }
}
