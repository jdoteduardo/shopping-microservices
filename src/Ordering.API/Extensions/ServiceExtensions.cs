using Ordering.API.Data;
using Ordering.API.Repositories;
using Ordering.API.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using EventBus.RabbitMQ.Extensions;
using System.Reflection;
using UserContext.Extensions;
using Microsoft.OpenApi.Models;

namespace Ordering.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // User Context
        services.AddUserContext();

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

        // EventBus (RabbitMQ)
        var rabbitMQSettings = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>()
            ?? new RabbitMQSettings();
        services.AddRabbitMQEventBus(rabbitMQSettings, clientName: "ordering_api");

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Ordering API",
                Version = "v1",
                Description = "Order Management Microservice API",
                Contact = new OpenApiContact
                {
                    Name = "ShopMicroservices",
                    Email = "support@shopmicroservices.com"
                }
            });

            // JWT Security Definition
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
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
