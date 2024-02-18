using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.Repositories;

public interface IRepository<TAggregate> where TAggregate : IAggregate<TAggregate>
{
    /// <summary>
    /// Creates and stores a single aggregate
    /// </summary>
    /// <param name="aggregate">The aggregate to store</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The stored aggregate</returns>
    Task<TAggregate> CreateAsync(TAggregate aggregate, CancellationToken ct);

    /// <summary>
    /// Get a single aggregate by its id
    /// </summary>
    /// <param name="id">The id of the aggregate to retrieve</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The stored aggregate</returns>
    Task<TAggregate> GetByIdAsync(IdOf<TAggregate> id, CancellationToken ct);

    /// <summary>
    /// Update a single aggregate by its id.
    /// </summary>
    /// <param name="id">The id of the aggregate to retrieve</param>
    /// <param name="mutator">The function called that should mutate the aggregate before saving</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The stored aggregate</returns>
    Task<TAggregate> UpdateByIdAsync(IdOf<TAggregate> id, Func<TAggregate, TAggregate> mutator, CancellationToken ct);

    /// <summary>
    /// Remove a single aggregate by its id
    /// </summary>
    /// <param name="id">The id of the aggregate to remove</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The stored aggregate</returns>
    Task<RemoveResult> RemoveByIdAsync(IdOf<TAggregate> id, CancellationToken ct);

    /// <summary>
    /// Retrieve all stored aggregates
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    /// <returns>An enumerable of aggregates</returns>
    IAsyncEnumerable<TAggregate> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Delete all stored instances for this repository.
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    Task Truncate(CancellationToken ct);

    /// <summary>
    /// Counts all aggregates of the given type.
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The total number of aggregates</returns>
    Task<int> TotalCountAsync(CancellationToken ct);
}
