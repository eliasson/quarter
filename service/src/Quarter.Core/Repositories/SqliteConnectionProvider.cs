using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Quarter.Core.Options;

namespace Quarter.Core.Repositories;

public interface ISqliteConnectionProvider
{
    string ConnectionString { get; }

    Task<SqliteConnection> GetConnectionAsync(CancellationToken ct);
}

public class DefaultSqliteConnectionProvider(IOptions<StorageOptions> options) : ISqliteConnectionProvider
{
    public string ConnectionString { get; } = options.Value.SqliteDatabase;

    public async Task<SqliteConnection> GetConnectionAsync(CancellationToken ct)
    {
        var conn = new SqliteConnection(ConnectionString);
        await conn.OpenAsync(ct);
        return conn;
    }
}
