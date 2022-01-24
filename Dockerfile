# Build part
FROM mcr.microsoft.com/dotnet/sdk:6.0.101 as build
WORKDIR /app
COPY ./ .
RUN dotnet publish  src/Quarter/Quarter.csproj --output /app/dist --configuration Release

# Runtime part
FROM mcr.microsoft.com/dotnet/aspnet:6.0.1
WORKDIR /app
COPY --from=build /app/dist .
ENTRYPOINT ["dotnet", "Quarter.dll"]