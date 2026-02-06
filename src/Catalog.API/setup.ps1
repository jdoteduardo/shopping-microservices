# Setup Script for Catalog.API
# Execute: .\setup.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Catalog.API Setup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check .NET 8 SDK
Write-Host "Checking .NET 8 SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "✗ .NET SDK not found. Please install .NET 8 SDK" -ForegroundColor Red
    exit 1
}

# Check EF Core tools
Write-Host "`nChecking EF Core tools..." -ForegroundColor Yellow
$efVersion = dotnet ef --version 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ EF Core tools found" -ForegroundColor Green
} else {
    Write-Host "✗ EF Core tools not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ EF Core tools installed successfully" -ForegroundColor Green
    } else {
        Write-Host "✗ Failed to install EF Core tools" -ForegroundColor Red
        exit 1
    }
}

# Restore packages
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Packages restored successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to restore packages" -ForegroundColor Red
    exit 1
}

# Build project
Write-Host "`nBuilding project..." -ForegroundColor Yellow
dotnet build --no-restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Project built successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Build failed" -ForegroundColor Red
    exit 1
}

# Ask about database setup
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Database Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Choose database option:" -ForegroundColor Yellow
Write-Host "1. SQL Server via Docker (Recommended)"
Write-Host "2. Use existing SQL Server"
Write-Host "3. Skip database setup"
$dbChoice = Read-Host "`nEnter choice (1-3)"

switch ($dbChoice) {
    "1" {
        Write-Host "`nChecking Docker..." -ForegroundColor Yellow
        $dockerVersion = docker --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Docker found: $dockerVersion" -ForegroundColor Green
            
            Write-Host "`nStarting SQL Server container..." -ForegroundColor Yellow
            docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" `
                -p 1433:1433 --name catalog-sqlserver `
                -d mcr.microsoft.com/mssql/server:2022-latest
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ SQL Server container started" -ForegroundColor Green
                Write-Host "  Waiting for SQL Server to be ready (30 seconds)..." -ForegroundColor Yellow
                Start-Sleep -Seconds 30
            } else {
                Write-Host "✗ Failed to start SQL Server container" -ForegroundColor Red
                Write-Host "  Container might already exist. Try: docker start catalog-sqlserver" -ForegroundColor Yellow
            }
        } else {
            Write-Host "✗ Docker not found. Please install Docker Desktop" -ForegroundColor Red
            exit 1
        }
    }
    "2" {
        Write-Host "`nUsing existing SQL Server" -ForegroundColor Green
        Write-Host "Make sure to update connection string in appsettings.json" -ForegroundColor Yellow
    }
    "3" {
        Write-Host "`nSkipping database setup" -ForegroundColor Yellow
    }
    default {
        Write-Host "`nInvalid choice. Skipping database setup" -ForegroundColor Yellow
    }
}

# Create and apply migrations
if ($dbChoice -ne "3") {
    Write-Host "`nDo you want to create and apply database migrations? (y/n)" -ForegroundColor Yellow
    $migrateChoice = Read-Host
    
    if ($migrateChoice -eq "y" -or $migrateChoice -eq "Y") {
        Write-Host "`nCreating migration..." -ForegroundColor Yellow
        
        # Check if migrations already exist
        if (Test-Path "Migrations") {
            Write-Host "  Migrations folder already exists. Skipping creation..." -ForegroundColor Yellow
        } else {
            dotnet ef migrations add InitialCreate
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Migration created successfully" -ForegroundColor Green
            } else {
                Write-Host "✗ Failed to create migration" -ForegroundColor Red
            }
        }
        
        Write-Host "`nApplying migrations to database..." -ForegroundColor Yellow
        dotnet ef database update
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Database updated successfully" -ForegroundColor Green
        } else {
            Write-Host "✗ Failed to update database" -ForegroundColor Red
            Write-Host "  Check your connection string in appsettings.json" -ForegroundColor Yellow
        }
    }
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Setup Complete!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "To run the application:" -ForegroundColor Green
Write-Host "  dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "To run with auto-reload:" -ForegroundColor Green
Write-Host "  dotnet watch run" -ForegroundColor White
Write-Host ""
Write-Host "Access Swagger UI at:" -ForegroundColor Green
Write-Host "  http://localhost:5001" -ForegroundColor White
Write-Host ""
Write-Host "Health check endpoint:" -ForegroundColor Green
Write-Host "  http://localhost:5001/health" -ForegroundColor White
Write-Host ""

# Ask if user wants to run the app now
Write-Host "Do you want to run the application now? (y/n)" -ForegroundColor Yellow
$runChoice = Read-Host

if ($runChoice -eq "y" -or $runChoice -eq "Y") {
    Write-Host "`nStarting application..." -ForegroundColor Green
    Write-Host "Press Ctrl+C to stop`n" -ForegroundColor Yellow
    dotnet run
}
