using ApiGateway.Middleware;
using Auth.Extensions;
using Auth.Models;
using Observability.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ApiGateway")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting ApiGateway");

    // Add Ocelot configuration files
    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    // Add services to the container
    builder.Services.AddControllers();
    
    // Add JWT Authentication using the Shared Auth Library
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() 
                     ?? throw new InvalidOperationException("JWT settings are not configured");
    
    builder.Services.AddJwtAuthentication(jwtSettings);

    // Add Ocelot and Polly
    builder.Services.AddOcelot(builder.Configuration)
                    .AddPolly();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Health Checks
    builder.Services.AddHealthChecks();

    // OpenTelemetry Observability
    builder.Services.AddObservability("api-gateway");

    var app = builder.Build();

    // Pipeline
    app.UseSerilogRequestLogging();

    app.UseCors("AllowAll");

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    // Custom Middleware to pass claims as headers to downstream services
    app.UseJwtClaimsMiddleware();
    
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapHealthChecks("/health");
        endpoints.MapPrometheusScrapingEndpoint();
    });

    // Use Ocelot
    await app.UseOcelot();

    Log.Information("ApiGateway started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ApiGateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

namespace ApiGateway
{
    public partial class Program { }
}
