# Catalog.API - Quick Reference Guide

## 🚀 Quick Start Commands

```powershell
# Clone and navigate
cd c:\CursoWebApi\projetos\shop-microservices\src\Catalog.API

# Automated setup (recommended for first time)
.\setup.ps1

# OR Manual setup
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

## 📦 Package Management

```powershell
# Restore packages
dotnet restore

# Add a package
dotnet add package PackageName

# Remove a package
dotnet remove package PackageName

# List packages
dotnet list package

# Update all packages
dotnet outdated  # (requires dotnet-outdated tool)
```

## 🔨 Build Commands

```powershell
# Build
dotnet build

# Build Release
dotnet build -c Release

# Clean
dotnet clean

# Clean + Build
dotnet clean && dotnet build

# Publish
dotnet publish -c Release -o ./publish
```

## ▶️ Run Commands

```powershell
# Run (Development)
dotnet run

# Run with watch (auto-reload on changes)
dotnet watch run

# Run with specific environment
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet run

# Run with specific port
dotnet run --urls="http://localhost:5002"
```

## 🗄️ Database & Migrations

```powershell
# Create migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Rollback to specific migration
dotnet ef database update PreviousMigrationName

# Remove last migration (not applied)
dotnet ef migrations remove

# List migrations
dotnet ef migrations list

# Drop database
dotnet ef database drop

# Drop and recreate
dotnet ef database drop --force
dotnet ef database update

# Generate SQL script
dotnet ef migrations script -o migration.sql

# View DbContext info
dotnet ef dbcontext info

# Generate DbContext from existing database (scaffold)
dotnet ef dbcontext scaffold "ConnectionString" Microsoft.EntityFrameworkCore.SqlServer -o Models
```

## 🐳 Docker Commands

```powershell
# Build image
docker build -t catalog-api:latest .

# Run container
docker run -d -p 5001:80 --name catalog-api catalog-api:latest

# Docker Compose - Build and start
docker-compose up -d --build

# Docker Compose - Start
docker-compose up -d

# Docker Compose - Stop
docker-compose stop

# Docker Compose - Down (remove containers)
docker-compose down

# Docker Compose - Down with volumes
docker-compose down -v

# View logs
docker-compose logs -f catalog-api

# Execute command in container
docker-compose exec catalog-api bash

# Apply migrations in container
docker-compose exec catalog-api dotnet ef database update
```

## 🧪 Testing Commands

```powershell
# Run tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Run specific test
dotnet test --filter "FullyQualifiedName~ProductsControllerTests"

# Run tests in watch mode
dotnet watch test
```

## 📊 Logging & Debugging

```powershell
# View logs (file)
Get-Content .\Logs\catalog-api-*.log -Wait

# View last 50 lines
Get-Content .\Logs\catalog-api-*.log -Tail 50

# Search logs
Select-String -Path .\Logs\*.log -Pattern "ERROR"

# Clear logs
Remove-Item .\Logs\*.log
```

## 🔍 Inspection & Info

```powershell
# Check .NET version
dotnet --version

# List SDKs
dotnet --list-sdks

# List runtimes
dotnet --list-runtimes

# Project info
dotnet --info

# EF Core version
dotnet ef --version
```

## 🌐 API Testing (PowerShell)

```powershell
# Health check
Invoke-RestMethod -Uri "http://localhost:5001/health"

# GET all products
Invoke-RestMethod -Uri "http://localhost:5001/api/products" -Method Get

# GET product by ID
Invoke-RestMethod -Uri "http://localhost:5001/api/products/1" -Method Get

# POST - Create product
$product = @{
    name = "Test Product"
    description = "Test Description"
    price = 19.99
    stock = 100
    categoryId = 1
    createdBy = "admin"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/api/products" `
    -Method Post `
    -Body $product `
    -ContentType "application/json"

# PUT - Update product
$update = @{
    name = "Updated Product"
    description = "Updated Description"
    price = 29.99
    stock = 50
    categoryId = 1
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5001/api/products/1" `
    -Method Put `
    -Body $update `
    -ContentType "application/json"

# DELETE product
Invoke-RestMethod -Uri "http://localhost:5001/api/products/1" -Method Delete
```

## 🛠️ Troubleshooting

```powershell
# Clear NuGet cache
dotnet nuget locals all --clear

# Rebuild with clean
dotnet clean
dotnet build --no-incremental

# Kill process using port 5001
Get-Process -Id (Get-NetTCPConnection -LocalPort 5001).OwningProcess | Stop-Process -Force

# Check if port is in use
Get-NetTCPConnection -LocalPort 5001

# Reset database completely
dotnet ef database drop --force
Remove-Item -Recurse -Force Migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# Fix SQL Server Docker issues
docker stop catalog-sqlserver
docker rm catalog-sqlserver
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" `
    -p 1433:1433 --name catalog-sqlserver `
    -d mcr.microsoft.com/mssql/server:2022-latest
```

## 📝 Code Generation

```powershell
# Add new controller
dotnet aspnet-codegenerator controller `
    -name NewController `
    -api `
    -outDir Controllers

# Install code generator (if not installed)
dotnet tool install -g dotnet-aspnet-codegenerator
```

## 🔧 Configuration

```powershell
# Set environment variable (current session)
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "your-connection-string"

# View environment variables
Get-ChildItem Env: | Where-Object { $_.Name -like "*ASPNET*" }

# User secrets (for development)
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "secret-value"
dotnet user-secrets list
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"
dotnet user-secrets clear
```

## 📈 Performance

```powershell
# Benchmark (requires BenchmarkDotNet)
dotnet run -c Release --project Benchmarks

# Memory profiling
dotnet-trace collect --process-id PID

# CPU profiling
dotnet-counters monitor --process-id PID
```

## 🔐 Security

```powershell
# Scan for vulnerabilities
dotnet list package --vulnerable

# Update vulnerable packages
dotnet add package PackageName

# Restore to specific version
dotnet add package PackageName --version 1.0.0
```

## 📦 Deployment

```powershell
# Publish self-contained
dotnet publish -c Release -r win-x64 --self-contained

# Publish framework-dependent
dotnet publish -c Release -r win-x64 --no-self-contained

# Create single file executable
dotnet publish -c Release -r win-x64 `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true
```

## 🎯 Useful Aliases (Add to PowerShell Profile)

```powershell
# Edit: notepad $PROFILE

function dr { dotnet run }
function dw { dotnet watch run }
function db { dotnet build }
function dt { dotnet test }
function dc { dotnet clean }
function dcu { dotnet ef database update }
function dcm { dotnet ef migrations add $args }
function dcd { dotnet ef database drop --force }

# Reload profile
. $PROFILE
```

## 📚 Documentation

```powershell
# Generate API documentation (requires tools)
dotnet tool install -g Swashbuckle.AspNetCore.Cli
swagger tofile --output api-docs.json bin/Debug/net8.0/Catalog.API.dll v1
```

## 🎨 Code Formatting

```powershell
# Format code (requires dotnet-format)
dotnet tool install -g dotnet-format
dotnet format

# Check formatting without fixing
dotnet format --verify-no-changes
```

---

## 💡 Quick Tips

1. **Always** use `dotnet watch run` during development for auto-reload
2. **Check** health endpoint before testing: `http://localhost:5001/health`
3. **Use** Swagger UI for interactive API testing: `http://localhost:5001`
4. **Keep** migrations in source control
5. **Never** commit `appsettings.Development.json` with sensitive data
6. **Use** user secrets for local development secrets
7. **Test** in Docker before deploying to ensure consistency

---

## 🔗 Useful URLs

- Swagger UI: `http://localhost:5001`
- Health Check: `http://localhost:5001/health`
- API Base: `http://localhost:5001/api`
- Products: `http://localhost:5001/api/products`
- Categories: `http://localhost:5001/api/categories`
