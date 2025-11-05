# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TestOrderService.API/TestOrderService.API.csproj", "TestOrderService.API/"]
COPY ["TestOrderService.Application/TestOrderService.Application.csproj", "TestOrderService.Application/"]
COPY ["TestOrderService.Domain/TestOrderService.Domain.csproj", "TestOrderService.Domain/"]
COPY ["TestOrderService.Infrastructure/TestOrderService.Infrastructure.csproj", "TestOrderService.Infrastructure/"]
RUN dotnet restore "./TestOrderService.API/TestOrderService.API.csproj"
COPY . .
WORKDIR "/src/TestOrderService.API"
RUN dotnet build "./TestOrderService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TestOrderService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestOrderService.API.dll"]