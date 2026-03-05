using System;
using Quarter.Core.UI.State;
using Quarter.State;
using Microsoft.Extensions.DependencyInjection;
using Quarter.Core.Commands;
using Quarter.Core.Migrations;
using Quarter.Core.Queries;
using Quarter.Core.Repositories;
using Quarter.HttpApi.Services;
using Quarter.Services;
using Microsoft.Data.Sqlite;

namespace Quarter;

public static class QuarterServiceConfiguration
{
    public static void UseQuarter(this IServiceCollection serviceCollection)
        => UseQuarter(serviceCollection, false);

    public static void UseQuarterUnderTest(this IServiceCollection serviceCollection)
        => UseQuarter(serviceCollection, true);

    private static void UseQuarter(this IServiceCollection serviceCollection, bool useInMemoryStorage)
    {
        if (useInMemoryStorage)
        {
            serviceCollection.AddSingleton<IRepositoryFactory, InMemoryRepositoryFactory>();
        }
        else
        {
            serviceCollection.AddSingleton<IPostgresConnectionProvider, DefaultPostgresConnectionProvider>();
            serviceCollection.AddSingleton<ISqliteConnectionProvider, DefaultSqliteConnectionProvider>();
            serviceCollection.ConfigureSqliteMigrations();

            var tempProvider = serviceCollection.BuildServiceProvider();
            var sqliteProvider = tempProvider.GetRequiredService<ISqliteConnectionProvider>();

            if (SqliteHasData(sqliteProvider.ConnectionString))
                serviceCollection.AddSingleton<IRepositoryFactory, SqliteRepositoryFactory>();
            else
                serviceCollection.AddSingleton<IRepositoryFactory, PostgresRepositoryFactory>();
        }

        serviceCollection.AddSingleton<ICommandHandler, CommandHandler>();
        serviceCollection.AddSingleton<IQueryHandler, QueryHandler>();
        serviceCollection.AddScoped<IStateManager<ApplicationState>>(
            sp =>
            {
                var repositoryFactory = sp.GetService<IRepositoryFactory>();
                var commandHandler = sp.GetService<ICommandHandler>();
                var authService = sp.GetService<IUserAuthorizationService>();

                return new StateManager<ApplicationState>(
                    new ApplicationState(),
                    new ActionHandler(repositoryFactory!, commandHandler!, authService!));
            });
        serviceCollection.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
        serviceCollection.AddSingleton<IApiService, ApiService>();
        serviceCollection.AddSingleton<IAdminService, AdminService>();
    }

    private static bool SqliteHasData(string connectionString)
    {
        try
        {
            using var conn = new SqliteConnection(connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM quser";
            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }
        catch
        {
            return false;
        }
    }
}
