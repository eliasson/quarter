using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using NUnit.Framework;

namespace Quarter.Core.UnitTest.Repositories
{
    /// <summary>
    /// Implements a common set of tests for all aggregate types regardless of storage implementation.
    /// </summary>
    /// <typeparam name="TAggregate">The aggregate type</typeparam>
    /// <typeparam name="TRepository">The aggregate repository type</typeparam>
    public abstract class RepositoryTestBase<TAggregate, TRepository>
        where TRepository : IRepository<TAggregate>
        where TAggregate : IAggregate<TAggregate>
    {
        /// <summary>
        /// Return a new (and empty) instance of a repository.
        /// </summary>
        /// <returns>A repository instance</returns>
        protected abstract TRepository Repository();

        /// <summary>
        /// Generate an arbitrary id for the given aggregate type.
        /// </summary>
        /// <returns>A new ID</returns>
        protected abstract IdOf<TAggregate> ArbitraryId();

        /// <summary>
        /// Generate an arbitrary aggregate.
        /// </summary>
        /// <returns>A new aggregate instance</returns>
        protected abstract TAggregate ArbitraryAggregate();

        /// <summary>
        /// A version of the aggregate with all timestamps set to null
        /// </summary>
        /// <param name="aggregate">The aggregate to modify</param>
        /// <returns>An updated version without timestamps</returns>
        protected abstract TAggregate WithoutTimestamps(TAggregate aggregate);

        /// <summary>
        /// Mutate the aggregate
        /// </summary>
        /// <param name="aggregate">The aggregate to mutate</param>
        /// <returns>The mutated aggregate</returns>
        protected abstract TAggregate Mutate(TAggregate aggregate);

        [SetUp]
        public async Task Setup()
        {
            var repository = Repository();
            await repository.Truncate(CancellationToken.None);
        }

        [Test]
        public void ThrowsIfAggregateDoesNotExist()
        {
            var repository = Repository();
            Assert.CatchAsync<NotFoundException>(() => repository.GetByIdAsync(ArbitraryId(), CancellationToken.None));
        }

        [Test]
        public async Task CanCreateAndGetAggregate()
        {
            var repository = Repository();
            var agg = ArbitraryAggregate();

            var storedAgg = await repository.CreateAsync(agg, CancellationToken.None);
            var readAgg = await repository.GetByIdAsync(storedAgg.Id, CancellationToken.None);

            Assert.That(WithoutTimestamps(readAgg), Is.EqualTo(WithoutTimestamps(agg)));
        }

        [Test]
        public async Task CreatedAggregateHasCreateTimestamp()
        {
            var nowInTest = UtcDateTime.Now();
            var repository = Repository();
            var agg = ArbitraryAggregate();

            var storedAgg = await repository.CreateAsync(agg, CancellationToken.None);

            Assert.That(storedAgg.Created.DateTime, Is.GreaterThan(nowInTest.DateTime));
        }

        [Test]
        public async Task CreatedReadCreateTimestamp()
        {
            var nowInTest = UtcDateTime.Now();
            var repository = Repository();
            var agg = ArbitraryAggregate();

            await repository.CreateAsync(agg, CancellationToken.None);
            var storedAgg = await repository.GetByIdAsync(agg.Id, CancellationToken.None);

            Assert.That(storedAgg.Created.DateTime, Is.GreaterThan(nowInTest.DateTime));
        }

        [Test]
        public void InstantiatedOnlyAggregateDoesNotHaveUpdatedTimestamp()
        {
            var agg = ArbitraryAggregate();

            Assert.That(agg.Updated, Is.Null);
        }

        [Test]
        public void ThrowsIfAggregateDoesNotExistWhenUpdating()
        {
            var repository = Repository();
            Assert.CatchAsync<NotFoundException>(() => repository.UpdateByIdAsync(ArbitraryId(), agg => agg, CancellationToken.None));
        }

        [Test]
        public async Task CanUpdateAggregate()
        {
            var repository = Repository();
            var agg = ArbitraryAggregate();

            var storedAgg = await repository.CreateAsync(agg, CancellationToken.None);
            var mutatedAgg = Mutate(storedAgg);
            await repository.UpdateByIdAsync(agg.Id, _ => mutatedAgg, CancellationToken.None);
            var readAgg = await repository.GetByIdAsync(agg.Id, CancellationToken.None);

            Assert.That(WithoutTimestamps(readAgg), Is.EqualTo(WithoutTimestamps(mutatedAgg)));
        }

        [Test]
        public async Task UpdatedAggregateHasCreateTimestamp()
        {
            var repository = Repository();
            var agg = ArbitraryAggregate();
            var nowInTest = UtcDateTime.Now();

            var storedAgg = await repository.CreateAsync(agg, CancellationToken.None);
            var mutatedAgg = Mutate(storedAgg);
            var updatedAgg = await repository.UpdateByIdAsync(agg.Id, _ => mutatedAgg, CancellationToken.None);

            Assert.That(updatedAgg.Updated?.DateTime, Is.GreaterThan(nowInTest.DateTime));
        }

        [Test]
        public async Task CanListAggregates()
        {
            var repository = Repository();
            var aggOne = ArbitraryAggregate();
            var aggTwo = ArbitraryAggregate();

            await repository.CreateAsync(aggOne, CancellationToken.None);
            await repository.CreateAsync(aggTwo, CancellationToken.None);

            var all = await repository.GetAllAsync(CancellationToken.None).ToListAsync(CancellationToken.None);
            Assert.That(all, Is.SupersetOf(new[] { aggOne, aggTwo }));
        }

        [Test]
        public async Task WhenTruncatingAllAggregatesAreDeleted()
        {
            var repository = Repository();
            var aggOne = ArbitraryAggregate();
            var aggTwo = ArbitraryAggregate();

            await repository.CreateAsync(aggOne, CancellationToken.None);
            await repository.CreateAsync(aggTwo, CancellationToken.None);
            await repository.Truncate(CancellationToken.None);
            var all = await repository.GetAllAsync(CancellationToken.None).ToListAsync(CancellationToken.None);

            Assert.That(all, Is.Empty);
        }

        [Test]
        public async Task CanDeleteAggregate()
        {
            var repository = Repository();
            var agg = ArbitraryAggregate();

            await repository.CreateAsync(agg, CancellationToken.None);
            var result = await repository.RemoveByIdAsync(agg.Id, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(RemoveResult.Removed));
                Assert.CatchAsync<NotFoundException>(() => repository.GetByIdAsync(agg.Id, CancellationToken.None));
            });
        }

        [Test]
        public async Task DeletingANonExistingAggIsANop()
        {
            var repository = Repository();
            var result = await repository.RemoveByIdAsync(ArbitraryId(), CancellationToken.None);

            Assert.That(result, Is.EqualTo(RemoveResult.NotRemoved));
        }

        [Test]
        public async Task DeletedAggregatesAreNotListed()
        {
            var repository = Repository();
            var agg = ArbitraryAggregate();

            await repository.CreateAsync(agg, CancellationToken.None);
            await repository.RemoveByIdAsync(agg.Id, CancellationToken.None);
            var all = await repository.GetAllAsync(CancellationToken.None).ToListAsync(CancellationToken.None);

            Assert.That(all, Does.Not.Contain(agg));
        }

        [Test]
        public async Task CanCountTotalAggregates()
        {
            var repository = Repository();
            var aggOne = ArbitraryAggregate();
            var aggTwo = ArbitraryAggregate();

            await repository.CreateAsync(aggOne, CancellationToken.None);
            await repository.CreateAsync(aggTwo, CancellationToken.None);

            var count = await repository.TotalCountAsync(CancellationToken.None);
            Assert.That(count, Is.EqualTo(2));
        }
    }
}
