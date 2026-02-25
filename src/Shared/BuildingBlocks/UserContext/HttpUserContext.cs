using Microsoft.AspNetCore.Http;

namespace UserContext;

/// <summary>
/// Reads authenticated user information from HTTP request headers
/// set by the API Gateway or authentication middleware.
/// 
/// Expected headers:
///   X-User-Id    — User's unique identifier
///   X-User-Email — User's email address
///   X-User-Roles — Comma-separated list of roles
/// </summary>
public class HttpUserContext : IUserContext
{
    private const string UserIdHeader = "X-User-Id";
    private const string UserEmailHeader = "X-User-Email";
    private const string UserRolesHeader = "X-User-Roles";

    private readonly IHttpContextAccessor _httpContextAccessor;

    // Lazy-initialized cached values for thread safety within a single request
    private string? _userId;
    private string? _email;
    private List<string>? _roles;
    private bool _initialized;
    private readonly object _lock = new();

    public HttpUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor
            ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public string? UserId
    {
        get
        {
            EnsureInitialized();
            return _userId;
        }
    }

    /// <inheritdoc />
    public string? Email
    {
        get
        {
            EnsureInitialized();
            return _email;
        }
    }

    /// <inheritdoc />
    public List<string> Roles
    {
        get
        {
            EnsureInitialized();
            return _roles!;
        }
    }

    /// <inheritdoc />
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(UserId);

    /// <inheritdoc />
    public bool IsInRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return false;

        return Roles.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Thread-safe lazy initialization. Reads headers once per request scope
    /// and caches the values.
    /// </summary>
    private void EnsureInitialized()
    {
        if (_initialized)
            return;

        lock (_lock)
        {
            if (_initialized)
                return;

            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.Request?.Headers != null)
            {
                _userId = GetHeaderValue(httpContext, UserIdHeader);
                _email = GetHeaderValue(httpContext, UserEmailHeader);

                var rolesHeader = GetHeaderValue(httpContext, UserRolesHeader);
                _roles = !string.IsNullOrWhiteSpace(rolesHeader)
                    ? rolesHeader.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToList()
                    : new List<string>();
            }
            else
            {
                _roles = new List<string>();
            }

            _initialized = true;
        }
    }

    /// <summary>
    /// Safely extracts a header value, returning null if absent or empty.
    /// </summary>
    private static string? GetHeaderValue(HttpContext httpContext, string headerName)
    {
        if (httpContext.Request.Headers.TryGetValue(headerName, out var values))
        {
            var value = values.FirstOrDefault();
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        return null;
    }
}
