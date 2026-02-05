# ShopMicroservices

> Cloud-native microservices platform built with .NET 8

[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Enabled-blue)](https://www.docker.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen)](https://github.com)

---

## 📋 About

**ShopMicroservices** is a modern cloud-native e-commerce platform built on a microservices architecture. This project demonstrates industry best practices, distributed system patterns, and cloud-ready design principles using the latest .NET 8 framework.

The platform is designed to be **production-ready** and **cloud-agnostic**, prepared for deployment on Azure while maintaining the flexibility to run locally or on private infrastructure like Proxmox. It serves as a comprehensive reference implementation for building scalable, resilient, and maintainable microservices-based applications.

---

## ✨ Features

- **🔧 Independent Microservices** - Three domain-driven microservices (Catalog, Basket, Ordering)
- **📨 Event-Driven Architecture** - Asynchronous communication using RabbitMQ
- **🔐 JWT Authentication** - Secure token-based authentication and authorization
- **🌐 API Gateway** - Centralized entry point for client applications
- **🐳 Docker Containerization** - Fully containerized services with Docker Compose
- **📊 Monitoring & Observability** - Built-in health checks and monitoring capabilities
- **🚀 CI/CD Pipeline** - Automated build and deployment workflows

---

## 🏗️ Architecture

### Microservices

- **Catalog.API** - Manages product catalog, inventory, and product information
- **Basket.API** - Handles shopping cart operations and user basket management
- **Ordering.API** - Processes orders, manages order lifecycle and history

### Tech Stack

- **Backend**: .NET 8, ASP.NET Core Web API
- **Messaging**: RabbitMQ for event-driven communication
- **Authentication**: JWT Bearer tokens
- **API Gateway**: Reverse proxy and routing
- **Containerization**: Docker & Docker Compose
- **Monitoring**: Health checks and telemetry
- **Database**: (Configured per service - SQL Server/PostgreSQL/MongoDB)

---

## 🚀 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### Running the Application

1. **Clone the repository**

   ```bash
   git clone https://github.com/yourusername/shop-microservices.git
   cd shop-microservices
   ```

2. **Start all services with Docker Compose**

   ```bash
   docker-compose up -d
   ```

3. **Access the services**
   - API Gateway: `http://localhost:5000`
   - Catalog API: `http://localhost:5001`
   - Basket API: `http://localhost:5002`
   - Ordering API: `http://localhost:5003`
   - RabbitMQ Management: `http://localhost:15672` (guest/guest)

---

## 📁 Project Structure

```
shop-microservices/
├── src/
│   ├── ApiGateway/              # API Gateway service
│   ├── Catalog.API/             # Product catalog microservice
│   ├── Basket.API/              # Shopping basket microservice
│   ├── Ordering.API/            # Order management microservice
│   └── Shared/
│       └── BuildingBlocks/      # Shared libraries and utilities
│           ├── Auth/            # Authentication middleware
│           ├── EventBus/        # Event bus abstractions
│           ├── EventBus.RabbitMQ/  # RabbitMQ implementation
│           ├── IntegrationEvents/   # Event contracts
│           └── UserContext/     # User context management
├── tests/                       # Unit and integration tests
│   ├── Catalog.Tests/
│   ├── Basket.Tests/
│   ├── Ordering.Tests/
│   ├── Auth.Tests/
│   └── Integration.Tests/
├── deploy/                      # Deployment configurations
│   ├── monitoring/              # Monitoring setup
│   └── proxmox/                 # Proxmox deployment configs
├── docs/                        # Documentation
└── docker-compose.yml           # Docker Compose configuration
```

---

## 🛠️ Development

### Running Individual Services

Each microservice can be run independently for development:

```bash
cd src/Catalog.API
dotnet run
```

### Running Tests

```bash
dotnet test
```

### Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🤝 Acknowledgments

Built with ❤️ using modern cloud-native patterns and best practices.

For questions or support, please open an issue in the GitHub repository.
