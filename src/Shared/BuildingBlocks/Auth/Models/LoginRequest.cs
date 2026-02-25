namespace Auth.Models;

/// <summary>
/// Request model for user authentication.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's plain-text password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
