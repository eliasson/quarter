using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Quarter.Core.Options;

namespace Quarter.Core.Repositories;

public interface IPostgresConnectionProvider
{
    string ConnectionString { get; }

    Task<NpgsqlConnection> GetConnectionAsync(CancellationToken ct);
}

public sealed class DefaultPostgresConnectionProvider : IPostgresConnectionProvider, IAsyncDisposable, IDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public DefaultPostgresConnectionProvider(IOptions<StorageOptions> options, ILoggerFactory loggerFactory)
    {
        // Create the single data source used by the entire application.
        ConnectionString = options.Value.DefaultDatabase;
        _dataSource = new NpgsqlDataSourceBuilder(ConnectionString)
            .UseLoggerFactory(loggerFactory)
            .Build();
    }

    public string ConnectionString { get; }

    public async Task<NpgsqlConnection> GetConnectionAsync(CancellationToken ct)
        => await _dataSource.OpenConnectionAsync(ct);

    public ValueTask DisposeAsync()
        => _dataSource.DisposeAsync();

    public void Dispose()
        => _dataSource.Dispose();
}
