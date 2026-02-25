using Microsoft.Extensions.DependencyInjection;

namespace UserContext.Extensions;

/// <summary>
/// Extension methods for registering UserContext services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds IUserContext and its dependencies to the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        // IHttpContextAccessor is required for reading request headers
        services.AddHttpContextAccessor();

        // Register UserContext as scoped (one instance per HTTP request)
        services.AddScoped<IUserContext, HttpUserContext>();

        return services;
    }
}
