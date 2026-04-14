# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy project files first (for layer caching)
COPY src/SplashSphere.Domain/SplashSphere.Domain.csproj Domain/
COPY src/SplashSphere.Application/SplashSphere.Application.csproj Application/
COPY src/SplashSphere.Infrastructure/SplashSphere.Infrastructure.csproj Infrastructure/
COPY src/SplashSphere.SharedKernel/SplashSphere.SharedKernel.csproj SharedKernel/
COPY src/SplashSphere.API/SplashSphere.API.csproj API/

# Restore dependencies (cached unless .csproj changes)
RUN dotnet restore API/SplashSphere.API.csproj

# Copy everything and build
COPY src/ .
WORKDIR /src/API
RUN dotnet publish -c Release -o /app --no-restore

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Install ICU for globalization (needed for Philippine peso formatting)
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Copy published output
COPY --from=build /app .

# Create non-root user
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

# Railway injects PORT env var — Kestrel must bind to it
# IMPORTANT: Bind to 0.0.0.0, NOT localhost
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-5000}

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD wget -qO- http://localhost:${PORT:-5000}/health || exit 1

ENTRYPOINT ["dotnet", "SplashSphere.API.dll"]