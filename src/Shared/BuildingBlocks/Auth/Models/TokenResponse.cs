namespace Auth.Models;

/// <summary>
/// Response model containing the generated JWT token.
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// The JWT token string.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration date/time (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Token type, always "Bearer".
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
}
