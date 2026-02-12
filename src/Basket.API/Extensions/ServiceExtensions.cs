using Basket.API.Repositories;
using Basket.API.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using StackExchange.Redis;
using System.Reflection;

namespace Basket.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Redis - suporta tanto ConnectionStrings:Redis quanto Redis:ConnectionString
        var redisConnectionString = configuration["Redis:ConnectionString"] 
            ?? configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string is not configured");

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
            configurationOptions.AbortOnConnectFail = false;
            configurationOptions.ConnectRetry = 3;
            configurationOptions.ConnectTimeout = 5000;
            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        // Repositories
        services.AddScoped<IBasketRepository, BasketRepository>();

        // Services
        services.AddScoped<IBasketService, BasketService>();

        // AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Health Checks
        services.AddHealthChecks()
            .AddRedis(redisConnectionString, name: "redis", tags: new[] { "ready" });

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Basket API",
                Version = "v1",
                Description = "Shopping Basket Microservice API",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "ShopMicroservices",
                    Email = "support@shopmicroservices.com"
                }
            });

            // Include XML comments if available
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket API v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
