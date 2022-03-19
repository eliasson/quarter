using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Commands;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Commands;

public class AddActivityCommandTest : CommandTestBase
{
    public class WhenAddingActivity : AddActivityCommandTest
    {
        [OneTimeSetUp]
        public async Task AddingProject()
        {
            var command = new AddActivityCommand(IdOf<Project>.Random(), "Sample activity", "Something", Color.FromHexString("#000"));
            await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
        }

        [Test]
        public async Task ItShouldHaveAddedTheActivity()
        {
            var activities = await RepositoryFactory.ActivityRepository(ActingUser)
                .GetAllAsync(CancellationToken.None)
                .Select(a => a.Name)
                .ToListAsync();

            Assert.That(activities, Is.EquivalentTo(new [] { "Sample activity" }));
        }
    }
}