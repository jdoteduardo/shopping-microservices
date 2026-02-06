# Catalog.API - Product Catalog Microservice

## Overview

The Catalog API is a microservice responsible for managing product catalogs and categories in the ShopMicroservices platform.

## Features

- Product CRUD operations
- Category management
- Entity Framework Core with SQL Server
- Repository pattern
- AutoMapper for DTO mapping
- FluentValidation for request validation
- Serilog for structured logging
- Swagger/OpenAPI documentation
- Health checks

## Technology Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server
- AutoMapper
- FluentValidation
- Serilog
- Swashbuckle (Swagger)

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or full instance)

### Configuration

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=CatalogDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

### Running the API

1. **Restore packages**

   ```bash
   dotnet restore
   ```

2. **Apply database migrations**

   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

3. **Run the application**

   ```bash
   dotnet run
   ```

4. **Access Swagger UI**
   Open browser at: `http://localhost:5001`

## API Endpoints

### Products

- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/category/{categoryId}` - Get products by category
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

### Categories

- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get category by ID
- `POST /api/categories` - Create new category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category

### Health

- `GET /health` - Health check endpoint

## Project Structure

```
Catalog.API/
├── Controllers/           # API Controllers
├── Data/                  # DbContext and configurations
├── DTOs/                  # Data Transfer Objects
├── Extensions/            # Service extensions
├── Mapping/               # AutoMapper profiles
├── Middleware/            # Custom middleware
├── Models/                # Domain entities
├── Repositories/          # Repository interfaces and implementations
├── Validators/            # FluentValidation validators
├── Program.cs             # Application entry point
└── appsettings.json       # Configuration
```

## Database Migrations

Create a new migration:

```bash
dotnet ef migrations add MigrationName
```

Update database:

```bash
dotnet ef database update
```

Remove last migration:

```bash
dotnet ef migrations remove
```

## Logging

Logs are written to:

- Console (all environments)
- File: `Logs/catalog-api-{Date}.log` (rotating daily)

## Health Checks

Health check endpoint: `GET /health`

Returns:

- `Healthy` - Database connection successful
- `Unhealthy` - Database connection failed

## Error Handling

Global exception handling middleware catches all unhandled exceptions and returns appropriate HTTP status codes with error messages.

## Validation

All DTOs are validated using FluentValidation before processing requests.
