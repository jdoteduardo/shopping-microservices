using Auth.Exceptions;
using Auth.Models;
using Auth.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Auth.Tests;

public class AuthServiceTests
{
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _tokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _jwtSettings = new JwtSettings
        {
            ExpirationMinutes = 60
        };

        _sut = new AuthService(
            _tokenGeneratorMock.Object,
            _jwtSettings,
            _loggerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "Test@123"
        };

        _tokenGeneratorMock
            .Setup(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
            .Returns("fake-jwt-token");

        // Act
        var response = await _sut.LoginAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("fake-jwt-token", response.Token);
        Assert.Equal("Bearer", response.TokenType);
        Assert.True(response.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "WrongPassword"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(request));
        Assert.Equal("Invalid email or password.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "SomePassword"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(request));
        Assert.Equal("Invalid email or password.", ex.Message);
    }
}
