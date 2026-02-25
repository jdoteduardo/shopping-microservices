namespace Auth.Services;

/// <summary>
/// Interface for generating JWT tokens.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="userId">Unique identifier of the user.</param>
    /// <param name="email">User's email address.</param>
    /// <param name="roles">List of roles assigned to the user.</param>
    /// <returns>JWT token string.</returns>
    string GenerateToken(string userId, string email, List<string> roles);
}
