using Catalog.API.Data;
using Catalog.API.Extensions;
using Catalog.API.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Catalog.API")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Catalog.API");

    // Add services to the container
    builder.Services.AddControllers();

    // Custom service extensions
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.AddSwaggerDocumentation();

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

    var app = builder.Build();

    // Apply database migrations automatically
    await ApplyMigrationsAsync(app);

    // Configure the HTTP request pipeline
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerDocumentation();
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

    app.UseAuthorization();

    app.MapControllers();

    // Health check endpoint
    app.MapHealthChecks("/health");

    // API info endpoint
    app.MapGet("/info", () => new
    {
        service = "Catalog API",
        version = "1.0.0",
        status = "Running",
        timestamp = DateTime.UtcNow
    });

    Log.Information("Catalog.API started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Method to apply migrations automatically
static async Task ApplyMigrationsAsync(WebApplication app)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();

        Log.Information("🔄 Starting database migration process...");

        // Retry logic para aplicar migrations
        var retryCount = 0;
        const int maxRetries = 10;
        const int delayMilliseconds = 3000;

        while (retryCount < maxRetries)
        {
            try
            {
                retryCount++;
                Log.Information("🔌 Attempt {Attempt}/{MaxRetries} - Applying migrations...", retryCount, maxRetries);

                // MigrateAsync cria o banco se não existir e aplica todas as migrations
                await context.Database.MigrateAsync();

                Log.Information("✅ Database migrations applied successfully!");
                Log.Information("🎉 Database 'CatalogDb' is ready!");

                // Verificar migrations aplicadas
                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                Log.Information("📊 Total migrations applied: {Count}", appliedMigrations.Count());

                return; // Sucesso - sair do método
            }
            catch (Exception ex)
            {
                Log.Warning("❌ Migration attempt {Attempt}/{MaxRetries} failed: {Message}",
                    retryCount, maxRetries, ex.Message);

                if (retryCount >= maxRetries)
                {
                    Log.Error("❌ Failed to apply migrations after {MaxRetries} attempts", maxRetries);
                    Log.Warning("⚠️ Application will start WITHOUT database. Health checks will fail!");
                    return;
                }

                Log.Information("⏳ Waiting {Delay}ms before retry...", delayMilliseconds);
                await Task.Delay(delayMilliseconds);
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Critical error in migration process");
        Log.Warning("⚠️ Application will start, but database is NOT initialized");
    }
}