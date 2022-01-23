using System;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Quarter.Core.Repositories;

namespace Quarter.Core.Migrations;

public static class Configuration
{
    public static void ConfigureMigrations(this IServiceCollection serviceCollection)
    {
        var connectionProvider = serviceCollection.BuildServiceProvider().GetService<IPostgresConnectionProvider>();
        if (connectionProvider?.ConnectionString is null)
            throw new SystemException("Could not setup required connection provider service");

        serviceCollection.AddFluentMigratorCore()
            .ConfigureRunner(builder =>
            {
                builder
                    .AddPostgres()
                    .WithGlobalConnectionString(connectionProvider.ConnectionString)
                    .ScanIn(typeof(InitialMigration).Assembly).For.All();
            })
            .AddLogging(loggingBuilder => loggingBuilder.AddFluentMigratorConsole());

    }
}