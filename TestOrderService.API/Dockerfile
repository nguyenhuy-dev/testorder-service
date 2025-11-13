# Optimized for Google Kubernetes Engine (GKE)
# Using Ubuntu Chiseled images for better GCP compatibility and security

# Runtime stage - Ubuntu Chiseled is smaller and more secure
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage - using standard Ubuntu for build tools
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only project files first for better layer caching
COPY ["TestOrderService.Domain/TestOrderService.Domain.csproj", "TestOrderService.Domain/"]
COPY ["TestOrderService.Application/TestOrderService.Application.csproj", "TestOrderService.Application/"]
COPY ["TestOrderService.Infrastructure/TestOrderService.Infrastructure.csproj", "TestOrderService.Infrastructure/"]
COPY ["TestOrderService.API/TestOrderService.API.csproj", "TestOrderService.API/"]

# Restore dependencies - this layer is cached unless project files change
RUN dotnet restore "./TestOrderService.API/TestOrderService.API.csproj" --ignore-failed-sources

# Copy the rest of the source code
COPY . .

# Build and publish with optimizations for production
WORKDIR "/src/TestOrderService.API"
RUN dotnet publish "./TestOrderService.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false 

# Final stage - using Ubuntu Chiseled for minimal attack surface
FROM base AS final
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TestOrderService.API.dll"]