Quarter is built using .NET Core as backend and a Gleam based front-end using Gleam.

## Prerequisites

Make sure you have the following tools installed:

- [.NET SDK 9](https://dotnet.microsoft.com/download)
- [Node.js 20](https://nodejs.org/) (npm is included)
- [Gleam 1.14+](https://gleam.run/getting-started/installing/)
- [Erlang/OTP 28+](https://gleam.run/getting-started/installing/) (required by Gleam)
- [Docker](https://docs.docker.com/get-docker/) (for the PostgreSQL database)

## Getting started with development

1. Clone this repository:

`git clone https://github.com/eliasson/quarter.git`

2. Create a copy of the settings for you to customize:

`cp service/src/Quarter/appsettings.json service/src/Quarter/appsettings.Development.json`

By default Quarter will run in local mode, which is useful for development. See [authentication.md](authentication.md) for
details on how to setup external Identity Providers.

3. Start a PostgreSQL database

Start an instance of the PostgreSQL database using Docker Compose.

Run `docker compose -f tools/docker/docker-compose.yaml up -d`

_The first time this is run it will execute the `tools/docker/init.sql` script to setup the local database
and privileges. All tables are created using Fluent Migrations at `service/src/Quarter.Core/Migrations`._

4. Build the frontend

The backend serves static files from a `dist/` directory at the repository root. You need to build the
frontend before starting the backend.

From the `webapp/` directory:

```
npm ci
gleam build
npm run build
```

This compiles the Gleam code, bundles it with Vite, and outputs the result to `dist/` in the repository root.

During development you can use `npm run watch` (from `webapp/`) to automatically rebuild on changes.

5. Symlink the dist directory

The backend is configured to serve static files from `./dist` relative to where it runs
(`service/src/Quarter/`), but the frontend build outputs to `dist/` at the repository root.
You need to create a symlink so the backend can find the built frontend assets:

From the repository root:

```
ln -s ../../dist service/src/Quarter/dist
```

6. Build and run the backend

From the `service/` directory:

- `dotnet build` - download all libraries needed and build the solution
- `dotnet test` - runs unit tests for all projects
- `dotnet run --project src/Quarter` - starts the service at localhost

7. Open the application

Run `open https://localhost:5001/`, go to **Manage** section and create your first project and
some activities. Then head to **Timesheet** and start registering time!

## Configuration

By default Quarter will run in local mode, which is useful for development. The configuration
is located at `service/src/Quarter/appsettings.json` with development overrides in
`service/src/Quarter/appsettings.Development.json`.

See [authentication.md](authentication.md) for details on how to setup external Identity Providers.
