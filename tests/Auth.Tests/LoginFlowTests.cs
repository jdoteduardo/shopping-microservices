using System.Net;
using System.Net.Http.Json;
using Auth.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Auth.Tests;

public class LoginFlowTests : IClassFixture<WebApplicationFactory<ApiGateway.Program>>
{
    private readonly WebApplicationFactory<ApiGateway.Program> _factory;
    private readonly HttpClient _client;

    public LoginFlowTests(WebApplicationFactory<ApiGateway.Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidUser_Returns200WithToken()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "Test@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Login failed with {response.StatusCode}. Body: {content}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(tokenResponse);
        Assert.False(string.IsNullOrWhiteSpace(tokenResponse!.Token));
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithToken_Returns200()
    {
        // Arrange - First Login
        var loginRequest = new LoginRequest { Email = "user@example.com", Password = "Test@123" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var token = (await loginResponse.Content.ReadFromJsonAsync<TokenResponse>())!.Token;

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Call /me
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithoutToken_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AccessAdminEndpoint_WithUserRole_Returns403()
    {
        // Arrange - Login as regular user (user@example.com is just "User")
        var loginRequest = new LoginRequest { Email = "user@example.com", Password = "Test@123" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var token = (await loginResponse.Content.ReadFromJsonAsync<TokenResponse>())!.Token;

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Try to access admin-only route
        var response = await _client.GetAsync("/api/auth/admin-only");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}

