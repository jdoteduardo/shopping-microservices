# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/Basket.API/Basket.API.csproj", "src/Basket.API/"]
COPY ["src/Shared/BuildingBlocks/UserContext/UserContext.csproj", "src/Shared/BuildingBlocks/UserContext/"]
COPY ["src/Shared/BuildingBlocks/Observability/Observability.csproj", "src/Shared/BuildingBlocks/Observability/"]

RUN dotnet restore "src/Basket.API/Basket.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Basket.API"
RUN dotnet build "Basket.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Basket.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Install curl for healthchecks
RUN apk add --no-cache curl

# Create non-root user for security
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -D -s /bin/sh appuser

# Set ownership and permissions
RUN chown -R appuser:appgroup /app

EXPOSE 8080

# Copy published files
COPY --from=publish --chown=appuser:appgroup /app/publish .

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Basket.API.dll"]
