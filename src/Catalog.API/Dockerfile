# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/Catalog.API/Catalog.API.csproj", "src/Catalog.API/"]
COPY ["src/Shared/BuildingBlocks/EventBus/EventBus.csproj", "src/Shared/BuildingBlocks/EventBus/"]
COPY ["src/Shared/BuildingBlocks/EventBus.RabbitMQ/EventBus.RabbitMQ.csproj", "src/Shared/BuildingBlocks/EventBus.RabbitMQ/"]
COPY ["src/Shared/BuildingBlocks/IntegrationEvents/IntegrationEvents.csproj", "src/Shared/BuildingBlocks/IntegrationEvents/"]
COPY ["src/Shared/BuildingBlocks/UserContext/UserContext.csproj", "src/Shared/BuildingBlocks/UserContext/"]

RUN dotnet restore "src/Catalog.API/Catalog.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Catalog.API"
RUN dotnet build "Catalog.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Catalog.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for healthchecks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Catalog.API.dll"]
