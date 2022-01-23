using System;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.UnitTest.TestUtils;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Repositories
{
    public abstract class ActivityRepositoryTest : RepositoryTestBase<Activity, IActivityRepository>
    {
        protected override IdOf<Activity> ArbitraryId()
            => IdOf<Activity>.Random();

        protected override Activity ArbitraryAggregate()
        {
            var id = Guid.NewGuid();
            return new Activity(IdOf<Project>.Random(), $"Name {id}", $"Description {id}",
                Color.FromSystemColor(System.Drawing.Color.Aqua));
        }

        protected override Activity WithoutTimestamps(Activity aggregate)
            => aggregate;

        protected override Activity Mutate(Activity aggregate)
        {
            var id = Guid.NewGuid();
            aggregate.Description += id.ToString();
            return aggregate;
        }
    }

    public class InMemoryActivityRepositoryTest : ActivityRepositoryTest
    {
        protected override IActivityRepository Repository()
            => new InMemoryActivityRepository();
    }

    [Category(TestCategories.DatabaseDependency)]
    public class PostgresActivityRepositoryTest : ActivityRepositoryTest
    {
        protected override IActivityRepository Repository()
            => new PostgresActivityRepository(UnitTestPostgresConnectionProvider.Instance, IdOf<User>.Random());
    }
}