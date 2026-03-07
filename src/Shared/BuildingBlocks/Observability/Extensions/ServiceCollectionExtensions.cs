using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Observability.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenTelemetry observability (tracing + metrics) to the service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceName">The name of the service (e.g., "catalog-api").</param>
    /// <param name="configure">Optional action to add additional instrumentation.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        string serviceName,
        Action<ObservabilityOptions>? configure = null)
    {
        var options = new ObservabilityOptions();
        configure?.Invoke(options);

        // Configure OpenTelemetry Tracing
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: "1.0.0")
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        // Filter out health check and metrics endpoints to reduce noise
                        opts.Filter = httpContext =>
                            !httpContext.Request.Path.StartsWithSegments("/health") &&
                            !httpContext.Request.Path.StartsWithSegments("/metrics");
                    })
                    .AddHttpClientInstrumentation();

                // Add database-specific instrumentation
                if (options.UseEntityFrameworkCore)
                {
                    tracing.AddSource("Microsoft.EntityFrameworkCore");
                }

                if (options.UseMongoDB)
                {
                    tracing.AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources");
                }

                // Add custom sources
                foreach (var source in options.AdditionalSources)
                {
                    tracing.AddSource(source);
                }

                // Configure OTLP exporter if endpoint is provided
                var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();

                // Add custom meters
                foreach (var meter in options.AdditionalMeters)
                {
                    metrics.AddMeter(meter);
                }
            });

        return services;
    }
}

/// <summary>
/// Options for configuring OpenTelemetry observability.
/// </summary>
public class ObservabilityOptions
{
    /// <summary>
    /// Enable Entity Framework Core instrumentation (for SQL Server).
    /// </summary>
    public bool UseEntityFrameworkCore { get; set; }

    /// <summary>
    /// Enable MongoDB diagnostic source instrumentation.
    /// </summary>
    public bool UseMongoDB { get; set; }

    /// <summary>
    /// Additional tracing sources to listen for.
    /// </summary>
    public List<string> AdditionalSources { get; set; } = [];

    /// <summary>
    /// Additional meter names to capture.
    /// </summary>
    public List<string> AdditionalMeters { get; set; } = [];
}
