using System.Threading;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Quarter.Core.Migrations;
using Quarter.Core.Repositories;

namespace Quarter.Core.UnitTest.TestUtils;

public class UnitTestSqliteConnectionProvider : ISqliteConnectionProvider
{
    // Use a shared in-memory database that persists across connections for the same name
    public string ConnectionString { get; } = "Data Source=quarter_test;Mode=Memory;Cache=Shared";

    public static UnitTestSqliteConnectionProvider Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (_lock)
                {
                    _instance ??= new UnitTestSqliteConnectionProvider();
                    _instance.SetupInstance();
                }
            }

            return _instance;
        }
    }

    private static readonly object _lock = new();
    private static UnitTestSqliteConnectionProvider _instance;
    // Keep a persistent connection to prevent the in-memory DB from being dropped
    private static SqliteConnection _persistentConnection;

    private void SetupInstance()
    {
        _persistentConnection = new SqliteConnection(ConnectionString);
        _persistentConnection.Open();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ISqliteConnectionProvider>(_ => this);
        serviceCollection.ConfigureSqliteMigrations();
        var runner = serviceCollection
            .BuildServiceProvider(false)
            .GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    public async Task<SqliteConnection> GetConnectionAsync(CancellationToken ct)
    {
        var conn = new SqliteConnection(ConnectionString);
        await conn.OpenAsync(ct);
        return conn;
    }
}
