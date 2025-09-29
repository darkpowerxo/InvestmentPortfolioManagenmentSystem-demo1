# Investment Portfolio Management System - API Dockerfile
# Multi-stage build for .NET 9.0 API application

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files
COPY InvestmentPortfolioManager.sln ./
COPY src/InvestmentPortfolioManager.API/InvestmentPortfolioManager.API.csproj ./src/InvestmentPortfolioManager.API/
COPY src/InvestmentPortfolioManager.Application/InvestmentPortfolioManager.Application.csproj ./src/InvestmentPortfolioManager.Application/
COPY src/InvestmentPortfolioManager.Domain/InvestmentPortfolioManager.Domain.csproj ./src/InvestmentPortfolioManager.Domain/
COPY src/InvestmentPortfolioManager.Infrastructure/InvestmentPortfolioManager.Infrastructure.csproj ./src/InvestmentPortfolioManager.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/ ./src/

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish src/InvestmentPortfolioManager.API/InvestmentPortfolioManager.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Install dependencies for PDF generation (iText7) and Excel (EPPlus)
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libgdiplus \
        libc6-dev \
        fontconfig \
        fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create directories for file storage
RUN mkdir -p /app/reports /app/exports \
    && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Configure ASP.NET Core
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Expose port
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "InvestmentPortfolioManager.API.dll"]