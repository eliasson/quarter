using System.Threading;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Quarter.Core.Migrations;
using Quarter.Core.Repositories;

namespace Quarter.Core.UnitTest.TestUtils;

public class UnitTestPostgresConnectionProvider : IPostgresConnectionProvider
{
    public string ConnectionString { get; } =
        "Host=localhost;Port=5454;Username=sa01;Password=local;Database=quarter_test";

    public static UnitTestPostgresConnectionProvider Instance
    {
        get
        {
            if (_instance is null)
            {
                lock (_lock)
                {
                    _instance = new UnitTestPostgresConnectionProvider();
                    _instance.SetupInstance();
                }
            }

            return _instance;
        }
    }

    private static readonly object _lock = new();

    private static UnitTestPostgresConnectionProvider _instance;

    private void SetupInstance()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IPostgresConnectionProvider>(_ => this);
        serviceCollection.ConfigureMigrations();
        var runner = serviceCollection
            .BuildServiceProvider(false)
            .GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    public async Task<NpgsqlConnection> GetConnectionAsync(CancellationToken ct)
    {
        var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync(ct);
        return conn;
    }
}
