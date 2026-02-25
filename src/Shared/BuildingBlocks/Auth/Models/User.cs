namespace Auth.Models;

/// <summary>
/// Mock user model for demonstration purposes.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier of the user.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// BCrypt-hashed password.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// List of roles assigned to the user.
    /// </summary>
    public List<string> Roles { get; set; } = new();
}
