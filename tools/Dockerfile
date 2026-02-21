# Build part
FROM mcr.microsoft.com/dotnet/sdk:9.0 as build

# Install Node.js and Gleam for frontend build
RUN apt-get update && apt-get install -y curl wget unzip && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    rm -rf /var/lib/apt/lists/*

# Install Gleam binary directly
RUN wget https://github.com/gleam-lang/gleam/releases/download/v1.14.0/gleam-v1.14.0-x86_64-unknown-linux-musl.tar.gz && \
    tar -xzf gleam-v1.14.0-x86_64-unknown-linux-musl.tar.gz && \
    mv gleam /usr/local/bin/gleam && \
    chmod +x /usr/local/bin/gleam && \
    rm gleam-v1.14.0-x86_64-unknown-linux-musl.tar.gz

WORKDIR /app
COPY . .

# Build frontend
WORKDIR /app/webapp
RUN npm install
RUN gleam build
RUN npm run build

# Build backend
WORKDIR /app
RUN dotnet publish service/src/Quarter/Quarter.csproj --output /app/build --configuration Release

# Copy frontend dist to backend output
RUN cp -r dist/* /app/build/dist

# Runtime part
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "Quarter.dll"]
