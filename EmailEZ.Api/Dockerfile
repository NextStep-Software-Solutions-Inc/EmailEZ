# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG ASPNETCORE_ENVIRONMENT=Production

WORKDIR /src
COPY ["EmailEZ.Api/EmailEZ.Api.csproj", "EmailEZ.Api/"]
COPY ["EmailEZ.Application/EmailEZ.Application.csproj", "EmailEZ.Application/"]
COPY ["EmailEZ.Domain/EmailEZ.Domain.csproj", "EmailEZ.Domain/"]
COPY ["EmailEZ.Infrastructure/EmailEZ.Infrastructure.csproj", "EmailEZ.Infrastructure/"]
RUN dotnet restore "./EmailEZ.Api/EmailEZ.Api.csproj"
COPY . .
WORKDIR "/src/EmailEZ.Api"
RUN dotnet build "./EmailEZ.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG ASPNETCORE_ENVIRONMENT=Production
RUN dotnet publish "./EmailEZ.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ARG ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT
ENTRYPOINT ["dotnet", "EmailEZ.Api.dll"]