# Multi-stage build for self-contained FilesAPI with embedded database
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for dependency resolution
COPY ["FilesAPI/FilesAPI.csproj", "FilesAPI/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["Models/Models.csproj", "Models/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]

# Restore dependencies
RUN dotnet restore "FilesAPI/FilesAPI.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/FilesAPI"
RUN dotnet build "FilesAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application as framework-dependent
RUN dotnet publish "FilesAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    --no-restore

# Final runtime stage - use minimal base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install curl for health checks (optional)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create directories for application data and analytics
RUN mkdir -p /app/uploads /app/data /app/logs /app/wwwroot

# Copy published application
COPY --from=build /app/publish .

# Ensure analytics dashboard is available
RUN ls -la /app/wwwroot/ || echo "wwwroot directory contents:"

# Set environment variables for self-contained operation
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Configure for embedded database (LiteDB) instead of MongoDB
ENV USE_EMBEDDED_DATABASE=true
ENV DATABASE_PATH=/app/data/filesapi.db
ENV UPLOADS_PATH=/app/uploads

# Analytics configuration
ENV ANALYTICS_ENABLED=true
ENV ANALYTICS_RETENTION_DAYS=365

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Expose port
EXPOSE 8080

ENTRYPOINT ["dotnet", "FilesAPI.dll"]
