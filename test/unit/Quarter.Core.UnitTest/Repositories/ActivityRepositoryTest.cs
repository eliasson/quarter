using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            => ArbitraryAggregate(IdOf<Project>.Random());

        private Activity ArbitraryAggregate(IdOf<Project> projectId)
        {
            var id = Guid.NewGuid();
            return new Activity(projectId, $"Name {id}", $"Description {id}",
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

        [Test]
        public async Task ItShouldCreateASandboxActivity()
        {
            var repository = Repository();
            var projectId = IdOf<Project>.Random();
            await repository.CreateSandboxActivityAsync(projectId, CancellationToken.None);
            var activity = (await repository.GetAllAsync(CancellationToken.None).ToListAsync())
                .Single();

            Assert.Multiple(() =>
            {
                Assert.That(activity.ProjectId, Is.EqualTo(projectId));
                Assert.That(activity.Name, Is.EqualTo("Your first activity"));
                Assert.That(activity.Description, Is.EqualTo("An activity is used to register time. Each activity has a color to make it easier to associate to."));
                Assert.That(activity.Color.ToHex(), Is.EqualTo("#4E4694"));
            });
        }

        [Test]
        public async Task ItShouldOnlyReadActivitiesForTheGivenProject()
        {
            var repository = Repository();
            var projectIdOne = IdOf<Project>.Random();
            var projectIdTwo = IdOf<Project>.Random();
            var activityForProjectOne = ArbitraryAggregate(projectIdOne);
            var activityForProjectTwo = ArbitraryAggregate(projectIdTwo);

            await repository.CreateAsync(activityForProjectOne, CancellationToken.None);
            await repository.CreateAsync(activityForProjectTwo, CancellationToken.None);

            var allForOne =  await (repository.GetAllForProjectAsync(projectIdOne, CancellationToken.None).Select(a => a.Name).ToListAsync());
            var allForTwo =  await (repository.GetAllForProjectAsync(projectIdTwo, CancellationToken.None).Select(a => a.Name).ToListAsync());

            Assert.Multiple(() =>
            {
                Assert.That(allForOne, Is.EqualTo(new [] { activityForProjectOne.Name }));
                Assert.That(allForTwo, Is.EqualTo(new [] { activityForProjectTwo.Name }));
            });
        }
    }

    [TestFixture]
    public class InMemoryActivityRepositoryTest : ActivityRepositoryTest
    {
        protected override IActivityRepository Repository()
            => new InMemoryActivityRepository();
    }

    [TestFixture]
    [Category(TestCategories.DatabaseDependency)]
    public class PostgresActivityRepositoryTest : ActivityRepositoryTest
    {
        protected override IActivityRepository Repository()
            => new PostgresActivityRepository(UnitTestPostgresConnectionProvider.Instance, IdOf<User>.Random());
    }
}