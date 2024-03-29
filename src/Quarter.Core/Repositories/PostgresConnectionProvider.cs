using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Npgsql;
using Quarter.Core.Options;

namespace Quarter.Core.Repositories;

public interface IPostgresConnectionProvider
{
    string ConnectionString { get; }

    Task<NpgsqlConnection> GetConnectionAsync(CancellationToken ct);
}

public class DefaultPostgresConnectionProvider(IOptions<StorageOptions> options) : IPostgresConnectionProvider
{
    public string ConnectionString { get; } = options.Value.DefaultDatabase;

    public async Task<NpgsqlConnection> GetConnectionAsync(CancellationToken ct)
    {
        var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync(ct);
        return conn;
    }
}
