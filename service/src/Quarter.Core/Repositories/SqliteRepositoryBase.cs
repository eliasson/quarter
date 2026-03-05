using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.Repositories;

public abstract class SqliteRepositoryBase<T> : IRepository<T> where T : IAggregate<T>
{
    protected virtual IEnumerable<string> AdditionalColumns()
        => Array.Empty<string>();

    protected virtual object AdditionalColumnValue(string columnName, T aggregate)
        => throw new NotImplementedException($"Additional column named [{columnName}] is not implemented");

    protected virtual SqliteParameter? WithAccessCondition()
        => null;

    protected virtual Task PostGetByIdAsync(SqliteConnection connection, T aggregate, CancellationToken ct)
        => Task.CompletedTask;

    protected virtual Task PostCreateAsync(SqliteConnection connection, T aggregate, CancellationToken ct)
        => Task.CompletedTask;

    protected virtual Task PostUpdateByIdAsync(SqliteConnection connection, T aggregate, CancellationToken ct)
        => Task.CompletedTask;

    protected readonly ISqliteConnectionProvider _connectionProvider;
    private readonly string _tableName;
    private readonly string _aggregateName;
    private readonly string _additionalColumnNames;
    private readonly string _additionalColumnValueNames;
    private readonly string _additionalColumns;

    protected SqliteRepositoryBase(ISqliteConnectionProvider connectionProvider, string tableName, string aggregateName)
    {
        _connectionProvider = connectionProvider;
        _tableName = tableName;
        _aggregateName = aggregateName;
        var additional = AdditionalColumns().ToList();
        _additionalColumnNames = additional.Count == 0
            ? ""
            : ", " + string.Join(",", additional.Select(c => $"{c}"));
        _additionalColumnValueNames = additional.Count == 0
            ? ""
            : ", " + string.Join(",", additional.Select(c => $"@{c}"));
        _additionalColumns = additional.Count == 0
            ? ""
            : ", " + string.Join(",", additional.Select(c => $"{c}=@{c}"));
    }

    public async Task<T> CreateAsync(T aggregate, CancellationToken ct)
    {
        var stmt = $"INSERT INTO {_tableName} (id, data{_additionalColumnNames}) VALUES (@id, @data{_additionalColumnValueNames})";
        var parameters = WithAdditionalColumnValues(
            new SqliteParameter[]
            {
                new("id", aggregate.Id.Id.ToString()),
                new("data", await Serialize(aggregate))
            }, aggregate);

        try
        {
            await using var connection = await _connectionProvider.GetConnectionAsync(ct);
            var result = await ExecuteStatementAsync(connection, stmt, ct, parameters);

            if (result != 1)
                throw new ArgumentException($"Could not store new aggregate into table {_tableName}");

            await using var connection2 = await _connectionProvider.GetConnectionAsync(ct);
            await PostCreateAsync(connection2, aggregate, ct);
            return aggregate;
        }
        catch (SqliteException)
        {
            throw new ArgumentException($"Could not store new aggregate into table {_tableName}");
        }
    }

    public async Task<T> GetByIdAsync(IdOf<T> id, CancellationToken ct)
    {
        var accessCondition = WithAccessCondition();
        var accessClaus = accessCondition is null
            ? string.Empty
            : $"AND ({accessCondition.ParameterName})=@{accessCondition.ParameterName}";

        var query = $"SELECT data FROM {_tableName} WHERE (id)=@id {accessClaus};";

        var parameters = new List<SqliteParameter> { new("id", id.Id.ToString()) };
        if (accessCondition is not null)
            parameters.Add(accessCondition);

        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        var result = await ExecuteQueryAsync(connection, query, ct, parameters).ToListAsync(ct);
        if (!result.Any())
            throw new NotFoundException($"No {_aggregateName} with id {id} found");

        var agg = result.First();
        await PostGetByIdAsync(connection, agg, ct);
        return agg;
    }

    public async Task<T> UpdateByIdAsync(IdOf<T> id, Func<T, T> mutator, CancellationToken ct)
    {
        var accessCondition = WithAccessCondition();
        var accessClaus = accessCondition is null
            ? string.Empty
            : $"AND ({accessCondition.ParameterName})=@{accessCondition.ParameterName}";

        var stmt = $"UPDATE {_tableName} SET data=@data{_additionalColumns} WHERE id=@id {accessClaus}";

        var currentAggregate = await GetByIdAsync(id, ct);
        var updatedAggregate = mutator(currentAggregate);
        updatedAggregate.Updated = UtcDateTime.Now();

        var parameters = WithAdditionalColumnValues(
            new SqliteParameter[]
            {
                new("id", id.Id.ToString()),
                new("data", await Serialize(updatedAggregate))
            }, updatedAggregate);

        if (accessCondition is not null)
            parameters.Add(accessCondition);

        try
        {
            await using var connection = await _connectionProvider.GetConnectionAsync(ct);
            var result = await ExecuteStatementAsync(connection, stmt, ct, parameters);

            if (result != 1)
                throw new ArgumentException($"Could not store aggregate with ID {id.AsString()} for table {_tableName}");

            await using var connection2 = await _connectionProvider.GetConnectionAsync(ct);
            await PostUpdateByIdAsync(connection2, updatedAggregate, ct);
            return updatedAggregate;
        }
        catch (SqliteException)
        {
            throw new ArgumentException($"Could not store aggregate with ID {id.AsString()} for table {_tableName}");
        }
    }

    public Task<RemoveResult> RemoveByIdAsync(IdOf<T> id, CancellationToken ct)
    {
        var accessCondition = WithAccessCondition();
        var accessClaus = accessCondition is null
            ? string.Empty
            : $"AND ({accessCondition.ParameterName})=@{accessCondition.ParameterName}";

        var parameters = new List<SqliteParameter> { new("id", id.Id.ToString()) };
        if (accessCondition is not null)
            parameters.Add(accessCondition);

        return ExecuteRemoveStatement($"DELETE FROM {_tableName} WHERE id=@id {accessClaus};",
            parameters.ToArray(), ct);
    }

    public async IAsyncEnumerable<T> GetAllAsync([EnumeratorCancellation] CancellationToken ct)
    {
        var accessCondition = WithAccessCondition();
        var accessClaus = accessCondition is null
            ? string.Empty
            : $"WHERE ({accessCondition.ParameterName})=@{accessCondition.ParameterName}";

        var query = $"SELECT data FROM {_tableName} {accessClaus}";

        var parameters = new List<SqliteParameter>();
        if (accessCondition is not null)
            parameters.Add(accessCondition);

        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        await foreach (var agg in ExecuteQueryAsync(connection, query, ct, parameters))
        {
            await PostGetByIdAsync(connection, agg, ct);
            yield return agg;
        }
    }

    public virtual Task Truncate(CancellationToken ct)
        => TruncateTableAsync(_tableName, ct);

    public async Task<int> TotalCountAsync(CancellationToken ct)
    {
        var query = $"SELECT COUNT(*) FROM {_tableName};";

        await using var conn = await _connectionProvider.GetConnectionAsync(ct);
        var command = conn.CreateCommand();
        command.CommandText = query;

        return Convert.ToInt32(await command.ExecuteScalarAsync(ct));
    }

    protected async Task TruncateTableAsync(string tableName, CancellationToken ct)
    {
        var stmt = $"DELETE FROM {tableName};";
        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        _ = await ExecuteStatementAsync(connection, stmt, ct, Array.Empty<SqliteParameter>());
    }

    private async Task<int> ExecuteStatementAsync(
        SqliteConnection connection,
        string query,
        CancellationToken ct,
        IEnumerable<SqliteParameter> parameters)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = query;
        foreach (var p in parameters.GroupBy(p => p.ParameterName).Select(g => g.First()))
            command.Parameters.AddWithValue(p.ParameterName, p.Value);

        var result = await command.ExecuteNonQueryAsync(ct);
        await command.DisposeAsync();

        return result;
    }

    protected async IAsyncEnumerable<T> ExecuteQueryAsync(
        SqliteConnection connection,
        string query,
        [EnumeratorCancellation] CancellationToken ct,
        IEnumerable<SqliteParameter> parameters)
    {
        await using var conn = await _connectionProvider.GetConnectionAsync(ct);
        var command = conn.CreateCommand();

        command.CommandText = query;
        foreach (var p in parameters)
            command.Parameters.AddWithValue(p.ParameterName, p.Value);

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var json = reader.GetString(0);
            yield return await Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        }
    }

    protected async Task<RemoveResult> ExecuteRemoveStatement(string statement, IEnumerable<SqliteParameter> parameters, CancellationToken ct)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        var rowCount = await ExecuteStatementAsync(connection, statement, ct, parameters);

        return rowCount == 0
            ? RemoveResult.NotRemoved
            : RemoveResult.Removed;
    }

    private IList<SqliteParameter> WithAdditionalColumnValues(IEnumerable<SqliteParameter> parameters, T aggregate)
    {
        var additional = AdditionalColumns()
            .Select(column => new SqliteParameter($"{column}", AdditionalColumnValue(column, aggregate)));
        return parameters.Concat(additional).ToList();
    }

    private static async Task<string> Serialize(T aggregate)
    {
        await using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, aggregate);
        stream.Position = 0;
        var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static async Task<T> Deserialize(Stream stream)
    {
        var result = await JsonSerializer.DeserializeAsync<T>(stream);
        if (result == null)
            throw new ArgumentException($"Could not deserialize {nameof(T)} from stream");
        return result;
    }
}
