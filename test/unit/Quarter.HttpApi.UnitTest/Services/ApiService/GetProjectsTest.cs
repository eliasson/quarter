using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Utils;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class GetProjectsTest
{
    public class WhenNoProjectsExistForUser : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();

        [Test]
        public async Task ItShouldReturnAnEmptyResultForProjects()
        {
            var projects = await ApiService.ProjectsForUserAsync(_oc, CancellationToken.None).ToListAsync();
            Assert.That(projects, Is.Empty);
        }
    }

    public class WhenUserHasProjects : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();

        [OneTimeSetUp]
        public async Task Setup()
        {
            await AddProject(_oc.UserId, "Project alpha");
        }

        [Test]
        public async Task ItShouldContainProject()
        {
            var projectNames = await ApiService.ProjectsForUserAsync(_oc, CancellationToken.None)
                .Select(p => p.name)
                .ToListAsync();
            Assert.That(projectNames, Does.Contain("Project alpha"));
        }
    }
}