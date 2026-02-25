using System.Security.Claims;

namespace ApiGateway.Middleware;

/// <summary>
/// Middleware to extract JWT claims and inject them as HTTP headers 
/// for downstream microservices (X-User-Id, X-User-Email, X-User-Roles).
/// </summary>
public class JwtClaimsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtClaimsMiddleware> _logger;

    public JwtClaimsMiddleware(RequestDelegate next, ILogger<JwtClaimsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? context.User.FindFirst("sub")?.Value;
            
            var email = context.User.FindFirst(ClaimTypes.Email)?.Value 
                       ?? context.User.FindFirst("email")?.Value;
            
            var roles = context.User.FindAll(ClaimTypes.Role)
                                   .Select(c => c.Value)
                                   .ToList();

            if (!string.IsNullOrEmpty(userId))
            {
                context.Request.Headers["X-User-Id"] = userId;
            }

            if (!string.IsNullOrEmpty(email))
            {
                context.Request.Headers["X-User-Email"] = email;
            }

            if (roles.Any())
            {
                context.Request.Headers["X-User-Roles"] = string.Join(",", roles);
            }

            _logger.LogDebug("Injected user headers for User: {UserId}", userId);
        }

        await _next(context);
    }
}

public static class JwtClaimsMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtClaimsMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtClaimsMiddleware>();
    }
}
