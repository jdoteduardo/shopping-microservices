using Auth.Models;

namespace Auth.Services;

/// <summary>
/// Interface for authentication operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>Token response with JWT and expiration.</returns>
    Task<TokenResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">Email address to search for.</param>
    /// <returns>User if found, null otherwise.</returns>
    Task<User?> GetUserByEmailAsync(string email);
}
