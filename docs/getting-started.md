Quarter is built using .NET Core and Blazor Server.

Blazor is a UI framework from Microsoft where the UI is built more or less only in C#
and HTML-like templates. Rendering takes place on the server and a WebSocket session
is established between each user and server. Render updates are then pushed to connected
clients when needed.

## Getting started with development

Install .NET Core 6 or greater from [here](https://dotnet.microsoft.com/download)

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


### Look and feel

The look and feel of the application is completely self-contained, no CSS-libraries are used.
The style sheets are implemented in [Sass](https://sass-lang.com/) (`.sass` is like `.scss`
but without curly braces and semi-colon).

In order to compile Sass to CSS you need NodeJS and NPM installed, from [here](https://nodejs.org/en/download/).
Then run the following commands:

```shell
cd src/Quarter.Style
npm install
npm run build
```

This will output the CSS file to the location `src/Quarter/wwwroot/quarter.css`
from where it is server by the server.

**NOTE:** The server does not need to be restarted when a new CSS file is built.
