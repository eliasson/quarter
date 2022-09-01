using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        [Test]
        public async Task ItShouldCreateASandboxProject()
        {
            var repository = Repository();
            await repository.CreateSandboxProjectAsync(CancellationToken.None);
            var project = (await repository.GetAllAsync(CancellationToken.None).ToListAsync())
                .Single();

            Assert.Multiple(() =>
            {
                Assert.That(project.Name, Is.EqualTo("Your first project"));
                Assert.That(project.Description, Is.EqualTo("A project is used to group activities."));
            });
        }
    }

    [TestFixture]
    public class InMemoryProjectRepositoryTest : ProjectRepositoryTest
    {
        protected override IProjectRepository Repository()
            => new InMemoryProjectRepository();
    }

    [TestFixture]
    [Category(TestCategories.DatabaseDependency)]
    public class PostgresProjectRepositoryTest : ProjectRepositoryTest
    {
        protected override IProjectRepository Repository()
            => new PostgresProjectRepository(UnitTestPostgresConnectionProvider.Instance, IdOf<User>.Random());
    }
}