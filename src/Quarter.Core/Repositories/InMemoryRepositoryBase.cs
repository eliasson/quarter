using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.Repositories
{
    public abstract class InMemoryRepositoryBase<T> : IRepository<T> where T : IAggregate<T>
    {
        protected readonly ConcurrentDictionary<IdOf<T>, T> Storage = new ();

        protected virtual void CheckConstraints(T aggregate)
        {
        }

        public Task<T> CreateAsync(T aggregate, CancellationToken ct)
        {
            CheckConstraints(aggregate);
            if (!Storage.TryAdd(aggregate.Id, aggregate))
                throw new InvalidOperationException("Could not insert aggregate");

            return Task.FromResult(aggregate);
        }

        public Task<T> GetByIdAsync(IdOf<T> id, CancellationToken ct)
        {
            return Storage.TryGetValue(id, out var user)
                ? Task.FromResult(user)
                : throw new NotFoundException($"Could not find {nameof(T)} {id.AsString()}");
        }

        public async Task<T> UpdateByIdAsync(IdOf<T> id, Func<T, T> mutator, CancellationToken ct)
        {
            var aggregate = await GetByIdAsync(id, ct);
            var updatedAggregate = mutator(aggregate);
            updatedAggregate.Updated = UtcDateTime.Now();
            CheckConstraints(updatedAggregate);
            if (Storage.TryUpdate(id, updatedAggregate, aggregate))
                return updatedAggregate;
            throw new InvalidOperationException($"Could not update {nameof(T)}");
        }

        public Task<RemoveResult> RemoveByIdAsync(IdOf<T> id, CancellationToken ct)
        {
            return Storage.TryRemove(id, out _)
                ? Task.FromResult(RemoveResult.Removed)
                : Task.FromResult(RemoveResult.NotRemoved);
        }

        public IAsyncEnumerable<T> GetAllAsync(CancellationToken ct)
        {
            return Storage.Values.ToAsyncEnumerable();
        }

        public Task Truncate(CancellationToken ct)
        {
            Storage.Clear();
            return Task.CompletedTask;
        }
    }
}