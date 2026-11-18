# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first so dependency layers are cached.
COPY StockSystem.sln ./
COPY src/StockSystem.Domain/*.csproj src/StockSystem.Domain/
COPY src/StockSystem.Application/*.csproj src/StockSystem.Application/
COPY src/StockSystem.Infrastructure/*.csproj src/StockSystem.Infrastructure/
COPY src/StockSystem.API/*.csproj src/StockSystem.API/
RUN dotnet restore src/StockSystem.API/StockSystem.API.csproj

COPY src/ src/
RUN dotnet publish src/StockSystem.API/StockSystem.API.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# libfontconfig1: required by the native graphics libraries used for PDF export.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .

# Run as the non-root user provided by the base image.
USER $APP_UID
ENV ASPNETCORE_ENVIRONMENT=Production
# Hosting platforms inject PORT; default to 8080 locally.
ENTRYPOINT ["/bin/sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet StockSystem.API.dll"]
