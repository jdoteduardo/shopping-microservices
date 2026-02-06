# Catalog.API - Architecture & Design Patterns

## 🏛️ Architecture Overview

### Layered Architecture

```
┌─────────────────────────────────────────────┐
│           Controllers Layer                  │
│  (API Endpoints, Request/Response handling)  │
└───────────────┬─────────────────────────────┘
                │
┌───────────────▼─────────────────────────────┐
│         Application Layer                    │
│  (DTOs, Validators, Mapping, Middleware)    │
└───────────────┬─────────────────────────────┘
                │
┌───────────────▼─────────────────────────────┐
│          Domain Layer                        │
│         (Models, Entities)                   │
└───────────────┬─────────────────────────────┘
                │
┌───────────────▼─────────────────────────────┐
│       Infrastructure Layer                   │
│  (Repositories, DbContext, Configurations)  │
└─────────────────────────────────────────────┘
```

## 🎯 Design Patterns Implemented

### 1. Repository Pattern

**Purpose:** Abstracts data access logic and provides a collection-like interface for accessing domain objects.

**Implementation:**

- `IProductRepository` / `ProductRepository`
- `ICategoryRepository` / `CategoryRepository`

**Benefits:**

- Separation of concerns
- Easier unit testing (mock repositories)
- Centralized data access logic
- Flexibility to change data source

### 2. Dependency Injection (DI)

**Purpose:** Inverts control of object creation and promotes loose coupling.

**Implementation:**

```csharp
// Registered in ServiceExtensions.cs
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<ICategoryRepository, CategoryRepository>();
```

**Benefits:**

- Testability
- Maintainability
- Flexibility

### 3. DTO Pattern (Data Transfer Objects)

**Purpose:** Transfer data between layers without exposing domain models.

**Implementation:**

- `ProductDto`, `CreateProductDto`, `UpdateProductDto`
- `CategoryDto`, `CreateCategoryDto`, `UpdateCategoryDto`

**Benefits:**

- API contract stability
- Reduced over-posting vulnerabilities
- Versioning support

### 4. Middleware Pattern

**Purpose:** Processing pipeline for HTTP requests.

**Implementation:**

- `ExceptionHandlingMiddleware` - Global exception handling

**Benefits:**

- Centralized cross-cutting concerns
- Clean separation of concerns
- Reusability

### 5. Configuration Pattern

**Purpose:** Fluent API for entity configuration.

**Implementation:**

- `ProductConfiguration`
- `CategoryConfiguration`

**Benefits:**

- Separation of mapping logic from entities
- Better organization
- Reusability

## 📦 Project Structure Explained

```
Catalog.API/
│
├── Controllers/              # API Endpoints
│   ├── ProductsController.cs
│   └── CategoriesController.cs
│
├── DTOs/                     # Data Transfer Objects
│   ├── ProductDto.cs         # Response DTOs
│   └── CategoryDto.cs
│
├── Models/                   # Domain Entities
│   ├── Product.cs
│   └── Category.cs
│
├── Data/                     # Data Access Layer
│   ├── CatalogContext.cs     # DbContext
│   └── Configurations/       # EF Core configurations
│       ├── ProductConfiguration.cs
│       └── CategoryConfiguration.cs
│
├── Repositories/             # Repository Pattern
│   ├── IProductRepository.cs
│   ├── ProductRepository.cs
│   ├── ICategoryRepository.cs
│   └── CategoryRepository.cs
│
├── Validators/               # FluentValidation
│   ├── CreateProductDtoValidator.cs
│   ├── UpdateProductDtoValidator.cs
│   ├── CreateCategoryDtoValidator.cs
│   └── UpdateCategoryDtoValidator.cs
│
├── Mapping/                  # AutoMapper Profiles
│   └── MappingProfile.cs
│
├── Middleware/               # Custom Middleware
│   └── ExceptionHandlingMiddleware.cs
│
├── Extensions/               # Service Extensions
│   └── ServiceExtensions.cs
│
└── Program.cs                # Application Entry Point
```

## 🔄 Request Flow

```
1. HTTP Request
   ↓
2. Middleware Pipeline (Exception Handling)
   ↓
3. Controller receives request
   ↓
4. FluentValidation validates DTO
   ↓
5. AutoMapper maps DTO → Entity
   ↓
6. Repository performs data operation
   ↓
7. Entity Framework executes SQL
   ↓
8. Repository returns Entity
   ↓
9. AutoMapper maps Entity → DTO
   ↓
10. Controller returns HTTP Response
    ↓
11. Serilog logs the request
```

## 🔐 Security Best Practices

### 1. Input Validation

- FluentValidation on all input DTOs
- Model state validation
- SQL injection prevention via EF Core parameterized queries

### 2. Error Handling

- Global exception middleware
- No sensitive information in error messages
- Structured logging

### 3. Database Security

- Parameterized queries (EF Core)
- Least privilege database user
- Connection string in configuration (not hardcoded)

## 📊 Database Design

### Entity Relationships

```
Category (1) ────── (N) Product
   │                     │
   └─ Id                 ├─ Id
   ├─ Name               ├─ Name
   └─ Description        ├─ Description
                         ├─ Price
                         ├─ Stock
                         ├─ CategoryId (FK)
                         ├─ CreatedBy
                         ├─ CreatedAt
                         └─ UpdatedAt
```

### Indexing Strategy

```sql
-- Products table
CREATE INDEX IX_Products_Name ON Products(Name);
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);

-- Categories table
CREATE UNIQUE INDEX IX_Categories_Name ON Categories(Name);
```

## 🎨 Code Quality Practices

### 1. SOLID Principles

**Single Responsibility Principle (SRP)**

- Each class has one reason to change
- Repositories handle data access only
- Controllers handle HTTP concerns only

**Open/Closed Principle (OCP)**

- Extension via interfaces (IRepository)
- Closed for modification, open for extension

**Liskov Substitution Principle (LSP)**

- Implementations can replace interfaces
- Repository implementations are interchangeable

**Interface Segregation Principle (ISP)**

- Focused interfaces (IProductRepository, ICategoryRepository)
- Clients depend only on methods they use

**Dependency Inversion Principle (DIP)**

- Depend on abstractions (interfaces)
- High-level modules don't depend on low-level modules

### 2. DRY (Don't Repeat Yourself)

- Reusable validators
- Shared mapping profiles
- Extension methods for service registration

### 3. Separation of Concerns

- Clear layer boundaries
- Each layer has distinct responsibility

### 4. Clean Code

- Meaningful names
- Small, focused methods
- Async/await for I/O operations
- Proper error handling

## 🧪 Testing Strategy

### Unit Tests (Not yet implemented)

```
Tests/
├── Controllers/
├── Repositories/
├── Validators/
└── Mapping/
```

### Integration Tests (Not yet implemented)

- Test full request/response cycle
- In-memory database
- WebApplicationFactory

## 🚀 Performance Considerations

### 1. Async/Await

- All I/O operations are async
- Non-blocking database calls

### 2. No Tracking Queries

- `.AsNoTracking()` for read-only queries
- Better performance for GET operations

### 3. Eager Loading

- `.Include()` for related entities
- Prevents N+1 query problem

### 4. Connection Pooling

- Enabled by default in EF Core
- Reuses database connections

## 📈 Scalability Considerations

### Horizontal Scaling

- Stateless API design
- Can run multiple instances
- Database connection pooling

### Future Enhancements

- Redis caching layer
- Event sourcing
- CQRS pattern
- Read replicas for queries

## 🔍 Monitoring & Observability

### Structured Logging (Serilog)

```csharp
_logger.LogInformation("Creating product {ProductName}", product.Name);
_logger.LogWarning("Product {ProductId} not found", id);
_logger.LogError(ex, "Failed to create product");
```

### Health Checks

- Database connectivity check
- `/health` endpoint for monitoring

### Metrics (Future)

- Request duration
- Error rates
- Database query performance

## 📝 API Versioning Strategy (Future)

```csharp
// URL versioning
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ProductsController : ControllerBase

// Header versioning
services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});
```

## 🔄 CI/CD Integration Points

1. **Build**: `dotnet build`
2. **Test**: `dotnet test`
3. **Migrations**: `dotnet ef database update`
4. **Publish**: `dotnet publish -c Release`
5. **Docker**: `docker build -t catalog-api .`

## 📖 References

- [Microsoft ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core)
- [Repository Pattern](https://docs.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
