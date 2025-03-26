# Build part
FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
WORKDIR /app
COPY ./ .
RUN dotnet publish  src/Quarter/Quarter.csproj --output /app/dist --configuration Release

# Runtime part
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/dist .
ENTRYPOINT ["dotnet", "Quarter.dll"]
