using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Commands;

[TestFixture]
public class AddProjectCommandTest : CommandTestBase
{
    public class WhenAddingProject : AddProjectCommandTest
    {
        [OneTimeSetUp]
        public async Task AddingProject()
        {
            var command = new AddProjectCommand("Sample project", "Something", Color.FromHexString("#457b9d"));
            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
        }

        [Test]
        public async Task ItShouldHaveAddedTheProject()
        {
            var projects = await RepositoryFactory.ProjectRepository(ActingUser)
                .GetAllAsync(CancellationToken.None)
                .Select(p => p.Name)
                .ToListAsync();

            Assert.That(projects, Is.EquivalentTo(new[] { "Sample project" }));
        }

        [Test]
        public async Task ItShouldHavePersistedTheColor()
        {
            var projects = await RepositoryFactory.ProjectRepository(ActingUser)
                .GetAllAsync(CancellationToken.None)
                .ToListAsync();

            Assert.That(projects.Single().Color, Is.EqualTo(Color.FromHexString("#457b9d")));
        }
    }
}
