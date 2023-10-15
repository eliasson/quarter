using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.Repositories;

public abstract class PostgresRepositoryBase<T> : IRepository<T> where T : IAggregate<T>
{
    /// <summary>
    /// Return a list of additional columns supported by the implementing repository.
    /// By default no columns are added.
    ///
    /// This is will be called from constructor, so object will not be fully constructed!
    /// </summary>
    /// <returns>A list of column names</returns>
    protected virtual IEnumerable<string> AdditionalColumns()
        => Array.Empty<string>();

    /// <summary>
    /// Fetch the value for a additional column  for a given aggregate.
    ///
    /// This is will be called from constructor, so object will not be fully constructed!
    /// </summary>
    /// <param name="columnName">The column name that will be updated</param>
    /// <param name="aggregate">The aggregate carrying the value</param>
    /// <returns>The column value to insert</returns>
    protected virtual object AdditionalColumnValue(string columnName, T aggregate)
        => throw new NotImplementedException($"Additional column named [{columnName}] is not implemented");

    protected virtual NpgsqlParameter? WithAccessCondition()
        => null;

    /// <summary>
    /// Called after the entity is retrieved, to allow for additional operations (e.g. timesheet use this to read
    /// slots from a different table).
    /// </summary>
    /// <param name="connection">The connection used</param>
    /// <param name="aggregate">The aggregate that was read</param>
    /// <param name="ct">The cancellation token</param>
    protected virtual Task PostGetByIdAsync(NpgsqlConnection connection, T aggregate, CancellationToken ct)
        => Task.CompletedTask;

    /// <summary>
    /// Called after the entity is created, to allow for additional operations (e.g. timesheet use this to save
    /// slots in a different table).
    /// </summary>
    /// <param name="connection">The connection used</param>
    /// <param name="aggregate">The aggregate that was saved</param>
    /// <param name="ct">The cancellation token</param>
    protected virtual Task PostCreateAsync(NpgsqlConnection connection, T aggregate, CancellationToken ct)
        => Task.CompletedTask;

    /// <summary>
    /// Called after the entity is updated, to allow for additional operations (e.g. timesheet use this to save
    /// slots in a different table).
    /// </summary>
    /// <param name="connection">The SQLite connection used</param>
    /// <param name="aggregate">The aggregate that was saved</param>
    /// <param name="ct">The cancellation token</param>
    protected virtual Task PostUpdateByIdAsync(NpgsqlConnection connection, T aggregate, CancellationToken ct)
        => Task.CompletedTask;

    protected readonly IPostgresConnectionProvider _connectionProvider;
    private readonly string _tableName;
    private readonly string _aggregateName;
    private readonly string _additionalColumnNames;
    private readonly string _additionalColumnValueNames;
    private readonly string _additionalColumns;

    protected PostgresRepositoryBase(IPostgresConnectionProvider connectionProvider, string tableName, string aggregateName)
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
            new NpgsqlParameter[]
            {
                new("id", aggregate.Id.Id),
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
        catch (NpgsqlException)
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

        var parameters = new List<NpgsqlParameter> { new ("id", id.Id) };
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
            new NpgsqlParameter[]
            {
                new("id", id.Id),
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
        catch (NpgsqlException)
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

        var parameters = new List<NpgsqlParameter> { new ("id", id.Id) };
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

        var parameters = new List<NpgsqlParameter>();
        if (accessCondition is not null)
            parameters.Add(accessCondition);

        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        await foreach (var agg in ExecuteQueryAsync(connection, query, ct, parameters))
        {
            // Inefficient! Add a method that allow for bulk reads in post-operations
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
        _ = await ExecuteStatementAsync(connection, stmt, ct, Array.Empty<NpgsqlParameter>());
    }

    private async Task<int> ExecuteStatementAsync(
        NpgsqlConnection connection,
        string query,
        CancellationToken ct,
        IEnumerable<NpgsqlParameter> parameters)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = query;
        command.Parameters.AddRange(parameters.ToArray());

        var result = await command.ExecuteNonQueryAsync(ct);
        await command.DisposeAsync();

        return result;
    }

    protected async IAsyncEnumerable<T> ExecuteQueryAsync(
        NpgsqlConnection connection,
        string query,
        [EnumeratorCancellation] CancellationToken ct,
        IEnumerable<NpgsqlParameter> parameters)
    {
        await using var conn = await _connectionProvider.GetConnectionAsync(ct);
        var command = conn.CreateCommand();

        command.CommandText = query;
        command.Parameters.AddRange(parameters.ToArray());

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            yield return await Deserialize(await reader.GetStreamAsync(0, ct));
        }
    }

    protected async Task<RemoveResult> ExecuteRemoveStatement(string statement, IEnumerable<NpgsqlParameter> parameters, CancellationToken ct)
    {
        await using var connection = await _connectionProvider.GetConnectionAsync(ct);
        var rowCount = await ExecuteStatementAsync(connection, statement, ct, parameters);

        return rowCount == 0
            ? RemoveResult.NotRemoved
            : RemoveResult.Removed;
    }

    private IList<NpgsqlParameter> WithAdditionalColumnValues(IEnumerable<NpgsqlParameter> parameters, T aggregate)
    {
        var additional = AdditionalColumns()
            .Select(column => new NpgsqlParameter($"{column}", AdditionalColumnValue(column, aggregate)));
        return  parameters.Concat(additional).ToList();
    }

    private static async Task<string> Serialize(T aggregate)
    {
        await using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, aggregate);
        stream.Position = 0; // TODO: Add Rewind extension method
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