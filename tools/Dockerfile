# Frontend build
FROM node:20 AS frontend-build
RUN curl -fsSL https://github.com/gleam-lang/gleam/releases/download/v1.14.0/gleam-v1.14.0-x86_64-unknown-linux-musl.tar.gz \
    | tar -xz -C /usr/local/bin
WORKDIR /frontend
COPY webapp/ .
RUN gleam build && npm ci && npm run build

# Backend build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /app
COPY service/ .
RUN dotnet publish src/Quarter/Quarter.csproj --output /app/publish --configuration Release

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=backend-build /app/publish .
COPY --from=frontend-build /dist ./dist
ENTRYPOINT ["dotnet", "Quarter.dll"]
