using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Quarter.Core.Repositories;

namespace Quarter.Core.Migrations;

public static class Configuration
{
    public static void ConfigureMigrations(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddFluentMigratorCore()
            .ConfigureRunner(builder =>
            {
                builder
                    .AddPostgres()
                    .WithGlobalConnectionString(sp => sp.GetRequiredService<IPostgresConnectionProvider>().ConnectionString)
                    .ScanIn(typeof(InitialMigration).Assembly).For.All();
            })
            .AddLogging(loggingBuilder => loggingBuilder.AddFluentMigratorConsole());
    }
}
