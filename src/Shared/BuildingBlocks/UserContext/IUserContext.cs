namespace UserContext;

/// <summary>
/// Provides access to the authenticated user's information
/// extracted from HTTP request headers.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// The authenticated user's unique identifier (from X-User-Id header).
    /// Null if no user is authenticated.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// The authenticated user's email address (from X-User-Email header).
    /// Null if no user is authenticated.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// The authenticated user's roles (from X-User-Roles header, comma-separated).
    /// Empty list if no roles are present.
    /// </summary>
    List<string> Roles { get; }

    /// <summary>
    /// Whether the current request has an authenticated user.
    /// True if UserId is present and non-empty.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Checks if the authenticated user has a specific role.
    /// </summary>
    /// <param name="role">Role name to check (case-insensitive).</param>
    /// <returns>True if the user has the specified role.</returns>
    bool IsInRole(string role);
}
