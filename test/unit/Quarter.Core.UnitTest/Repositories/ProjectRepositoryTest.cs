using System;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Repositories;
using Quarter.Core.UnitTest.TestUtils;

namespace Quarter.Core.UnitTest.Repositories
{
    public abstract class ProjectRepositoryTest : RepositoryTestBase<Project, IProjectRepository>
    {
        protected override IdOf<Project> ArbitraryId()
            => IdOf<Project>.Random();

        protected override Project ArbitraryAggregate()
        {
            var id = Guid.NewGuid();
            return new Project($"Name {id}", $"Description {id}");
        }

        protected override Project WithoutTimestamps(Project aggregate)
            => aggregate;

        protected override Project Mutate(Project aggregate)
        {
            var id = Guid.NewGuid();
            aggregate.Description += id.ToString();
            return aggregate;
        }
    }

    public class InMemoryProjectRepositoryTest : ProjectRepositoryTest
    {
        protected override IProjectRepository Repository()
            => new InMemoryProjectRepository();
    }

    [Category(TestCategories.DatabaseDependency)]
    public class PostgresProjectRepositoryTest : ProjectRepositoryTest
    {
        protected override IProjectRepository Repository()
            => new PostgresProjectRepository(UnitTestPostgresConnectionProvider.Instance, IdOf<User>.Random());
    }
}