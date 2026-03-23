Quarter is built using .NET Core as backend and a Gleam based front-end using Gleam.

## Getting started with development

Install .NET Core from [here](https://dotnet.microsoft.com/download)

1. Clone this repository:

`git clone https://github.com/eliasson/quarter.git`

2. Create a copy of the settings for you to customize:

`cp src/Quarter/appsettings.json src/Quarter/appsettings.Development.json`

By default Quarter will run in local mode, which is useful for development. See [authentication.md](authentication.md) for
details on how to setup external Identity Providers.

3. Start a PostgreSQL database

Start an instance of the PostgreSQL database using docker-compose.

Run `docker-compose -f docker/docker-compose.yaml up`

_The first time this is run it will execute the `docker/init.sql` script to setup the local database
and privileges. All tables are created using Fluent Migrations at `src/Quarter.Core/Migrations`

4. Build and run from the root directory:

- `dotnet build` - download all libraries needed
- `dotnet test` - runs unit-test for all projects
- `dotnet run --project src/Quarter` - starts the service at localhost

Run `open https://localhost:5001/` go to **Manage** section and create your first project
some activities. Then head to **Timesheet** and start register time!
