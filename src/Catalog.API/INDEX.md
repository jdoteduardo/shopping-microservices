# 📚 Catalog.API - Documentation Index

Welcome to the **Catalog.API** microservice documentation. This index will help you navigate through all available documentation files.

---

## 📖 Documentation Files

### 🎯 Getting Started

1. **[README.md](README.md)** - Project overview, features, and basic setup
   - Overview of Catalog.API
   - Features and technology stack
   - Project structure
2. **[SETUP.md](SETUP.md)** - Detailed setup instructions
   - Prerequisites checklist
   - Step-by-step setup guide
   - Database configuration options
   - Troubleshooting common issues
   - Data seeding information

3. **[setup.ps1](setup.ps1)** - Automated setup script
   - Interactive PowerShell script
   - Automates entire setup process
   - **Recommended for first-time setup**

### 🐳 Docker & Containerization

4. **[DOCKER.md](DOCKER.md)** - Docker and Docker Compose guide
   - Running with Docker Compose
   - Building Docker images
   - Container management
   - Production configurations
   - Troubleshooting Docker issues

5. **[Dockerfile](Dockerfile)** - Container build configuration
   - Multi-stage build
   - Optimized for production

6. **[docker-compose.yml](docker-compose.yml)** - Full stack orchestration
   - API + SQL Server
   - Network configuration
   - Health checks

### 🏗️ Architecture & Design

7. **[ARCHITECTURE.md](ARCHITECTURE.md)** - Architecture documentation
   - Layered architecture overview
   - Design patterns implemented
   - SOLID principles
   - Database design
   - Request flow diagrams
   - Performance considerations
   - Scalability strategies

### 🔧 Development & Operations

8. **[QUICK-REFERENCE.md](QUICK-REFERENCE.md)** - Command reference
   - Quick start commands
   - Build, run, test commands
   - Database & migration commands
   - Docker commands
   - API testing examples
   - Troubleshooting commands
   - PowerShell aliases

### 🧪 Testing

9. **[api-tests.http](api-tests.http)** - HTTP request collection
   - Ready-to-use API tests
   - All endpoints covered
   - Test scenarios
   - Use with VS Code REST Client extension

### ⚙️ Configuration Files

10. **[appsettings.json](appsettings.json)** - Production configuration
11. **[appsettings.Development.json](appsettings.Development.json)** - Development configuration
12. **[launchSettings.json](Properties/launchSettings.json)** - Launch profiles

---

## 🗺️ Quick Navigation Guide

### "I want to..."

#### **...get started quickly**

→ Run `.\setup.ps1` or follow **[SETUP.md](SETUP.md)**

#### **...understand the architecture**

→ Read **[ARCHITECTURE.md](ARCHITECTURE.md)**

#### **...run with Docker**

→ Follow **[DOCKER.md](DOCKER.md)**

#### **...find a specific command**

→ Check **[QUICK-REFERENCE.md](QUICK-REFERENCE.md)**

#### **...test the API**

→ Use **[api-tests.http](api-tests.http)** or Swagger at `http://localhost:5001`

#### **...troubleshoot an issue**

→ See troubleshooting sections in **[SETUP.md](SETUP.md)** or **[DOCKER.md](DOCKER.md)**

#### **...learn about design patterns used**

→ See Design Patterns section in **[ARCHITECTURE.md](ARCHITECTURE.md)**

---

## 📁 Project Structure Overview

```
Catalog.API/
│
├── 📄 Documentation
│   ├── README.md                  # Project overview
│   ├── SETUP.md                   # Setup guide
│   ├── DOCKER.md                  # Docker guide
│   ├── ARCHITECTURE.md            # Architecture docs
│   ├── QUICK-REFERENCE.md         # Command reference
│   ├── INDEX.md                   # This file
│   ├── api-tests.http             # API tests
│   └── setup.ps1                  # Setup script
│
├── 🎮 Controllers
│   ├── ProductsController.cs      # Products API
│   └── CategoriesController.cs    # Categories API
│
├── 📦 DTOs
│   ├── ProductDto.cs              # Product DTOs
│   └── CategoryDto.cs             # Category DTOs
│
├── 🏛️ Models
│   ├── Product.cs                 # Product entity
│   └── Category.cs                # Category entity
│
├── 💾 Data
│   ├── CatalogContext.cs          # DbContext
│   └── Configurations/            # EF configurations
│
├── 🔄 Repositories
│   ├── IProductRepository.cs
│   ├── ProductRepository.cs
│   ├── ICategoryRepository.cs
│   └── CategoryRepository.cs
│
├── ✅ Validators
│   ├── CreateProductDtoValidator.cs
│   ├── UpdateProductDtoValidator.cs
│   ├── CreateCategoryDtoValidator.cs
│   └── UpdateCategoryDtoValidator.cs
│
├── 🗺️ Mapping
│   └── MappingProfile.cs          # AutoMapper profile
│
├── 🔌 Middleware
│   └── ExceptionHandlingMiddleware.cs
│
├── 🔧 Extensions
│   └── ServiceExtensions.cs       # DI configuration
│
├── ⚙️ Configuration
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Properties/launchSettings.json
│
├── 🐳 Docker
│   ├── Dockerfile
│   └── docker-compose.yml
│
└── 🚀 Entry Point
    └── Program.cs                 # Application startup
```

---

## 🎓 Learning Path

**For beginners:**

1. Start with [README.md](README.md) to understand what this service does
2. Run [setup.ps1](setup.ps1) to get it running
3. Explore the API using Swagger UI at `http://localhost:5001`
4. Try the requests in [api-tests.http](api-tests.http)

**For developers:**

1. Read [ARCHITECTURE.md](ARCHITECTURE.md) to understand design decisions
2. Review the code following the project structure
3. Check [QUICK-REFERENCE.md](QUICK-REFERENCE.md) for commands
4. Read [DOCKER.md](DOCKER.md) for containerization

**For DevOps:**

1. Review [DOCKER.md](DOCKER.md) for deployment
2. Check health endpoints and monitoring setup
3. Review configuration files for environment setup
4. Check CI/CD integration points in [ARCHITECTURE.md](ARCHITECTURE.md)

---

## 🔗 External Resources

- [.NET 8 Documentation](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-8)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Docker Documentation](https://docs.docker.com)
- [Microservices Architecture](https://docs.microsoft.com/dotnet/architecture/microservices)

---

## 📞 Quick Support

**Issue:** Can't get it running
→ Check [SETUP.md](SETUP.md) troubleshooting section

**Issue:** Docker problems
→ Check [DOCKER.md](DOCKER.md) troubleshooting section

**Issue:** Need a specific command
→ Check [QUICK-REFERENCE.md](QUICK-REFERENCE.md)

**Issue:** Want to understand why something is designed a certain way
→ Check [ARCHITECTURE.md](ARCHITECTURE.md)

---

## ✅ Pre-flight Checklist

Before you start, make sure you have:

- [ ] .NET 8 SDK installed
- [ ] SQL Server available (Docker, LocalDB, or full instance)
- [ ] Docker Desktop (if using containers)
- [ ] Code editor (VS Code, Visual Studio, or Rider)
- [ ] Read [README.md](README.md) for overview
- [ ] Reviewed [SETUP.md](SETUP.md) for requirements

---

## 🚀 Quick Start (30 seconds)

```powershell
# 1. Navigate to project
cd c:\CursoWebApi\projetos\shop-microservices\src\Catalog.API

# 2. Run setup script
.\setup.ps1

# 3. Open Swagger UI
start http://localhost:5001
```

**That's it!** You should now have a running Catalog.API instance.

---

## 📝 Version History

- **v1.0.0** - Initial release
  - CRUD operations for Products and Categories
  - Repository pattern implementation
  - FluentValidation
  - AutoMapper
  - Serilog logging
  - Docker support
  - Health checks
  - Comprehensive documentation

---

**Happy coding! 🎉**

For questions or issues, check the troubleshooting sections in the documentation files above.
