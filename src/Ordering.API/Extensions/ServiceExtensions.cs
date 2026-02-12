using Ordering.API.Data;
using Ordering.API.Repositories;
using Ordering.API.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;

namespace Ordering.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // MongoDB Settings
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

        // MongoDB Context
        services.AddSingleton<MongoDbContext>();

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Services
        services.AddScoped<IOrderService, OrderService>();

        // AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Health Checks
        var mongoConnectionString = configuration.GetSection("MongoDbSettings:ConnectionString").Value
            ?? throw new InvalidOperationException("MongoDB connection string is not configured");
        
        services.AddHealthChecks()
            .AddMongoDb(mongoConnectionString, name: "mongodb", tags: new[] { "ready" });

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Ordering API",
                Version = "v1",
                Description = "Order Management Microservice API",
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
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering API v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
