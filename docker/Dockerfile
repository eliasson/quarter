# Build part
FROM mcr.microsoft.com/dotnet/sdk:6.0.101 as build
WORKDIR /app
COPY ./ .
RUN dotnet publish  src/Quarter/Quarter.Server.csproj --output /app/dist --configuration Release

# Runtime part
FROM mcr.microsoft.com/dotnet/aspnet:5.0.9
WORKDIR /app
COPY --from=build /app/dist .
ENTRYPOINT ["dotnet", "Quarter.Server.dll"]