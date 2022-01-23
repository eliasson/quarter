using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Events;

namespace Quarter.Core.UnitTest.Commands;

public abstract class AddProjectCommandTest : CommandTestBase<ProjectCreatedEvent>
{
    public class WhenAddingProject : AddProjectCommandTest
    {
        [OneTimeSetUp]
        public async Task AddingProject()
        {
            var command = new AddProjectCommand("Sample project", "Something");
            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
        }

        [Test]
        public async Task ItShouldHaveAddedTheProject()
        {
            var projects = await RepositoryFactory.ProjectRepository(ActingUser)
                .GetAllAsync(CancellationToken.None)
                .Select(p => p.Name)
                .ToListAsync();

            Assert.That(projects, Is.EquivalentTo(new [] { "Sample project" }));
        }

        [Test]
        public void ItShouldHaveDispatchedProjectCreatedEvent()
        {
            var ev = EventSubscriber.CollectedEvents.Single();

            Assert.That(ev.Project.Name, Is.EqualTo("Sample project"));
        }
    }
}