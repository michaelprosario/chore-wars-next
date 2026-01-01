# ChoreWars Dockerfile
# Multi-stage build for optimal image size

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy project files for dependency restoration
COPY src/ChoreWars.Core/*.csproj ./ChoreWars.Core/
COPY src/ChoreWars.Infrastructure/*.csproj ./ChoreWars.Infrastructure/
COPY src/ChoreWars.Web/*.csproj ./ChoreWars.Web/

# Restore dependencies
RUN dotnet restore ./ChoreWars.Web/ChoreWars.Web.csproj

# Copy all source files
COPY src/ChoreWars.Core/. ./ChoreWars.Core/
COPY src/ChoreWars.Infrastructure/. ./ChoreWars.Infrastructure/
COPY src/ChoreWars.Web/. ./ChoreWars.Web/

# Build and publish the application
WORKDIR /source/ChoreWars.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create a non-root user for security
RUN groupadd -r chorewars && useradd -r -g chorewars chorewars

# Create directory for SQLite database with proper permissions
RUN mkdir -p /app/data && chown -R chorewars:chorewars /app/data

# Copy published application from build stage
COPY --from=build /app/publish .

# Set ownership of application files
RUN chown -R chorewars:chorewars /app

# Switch to non-root user
USER chorewars

# Expose port
EXPOSE 8080

# Configure ASP.NET Core to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "ChoreWars.Web.dll"]
