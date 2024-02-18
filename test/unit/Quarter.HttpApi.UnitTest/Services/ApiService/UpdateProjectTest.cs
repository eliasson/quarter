using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class UpdateProjectTest
{
    [TestFixture]
    public class WhenInputIsValid : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private ProjectResourceOutput? _output;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var project = await AddProject(_oc.UserId, "Project Alpha");
            var input = new UpdateProjectResourceInput
            {
                name = "Project Alpha Updated",
                description = "Updated Description Alpha"
            };
            _output = await ApiService.UpdateProjectAsync(project.Id, input, _oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldReturnOutputResourceForProject()
            => Assert.That(_output?.name, Is.EqualTo("Project Alpha Updated"));

        [Test]
        public async Task ItShouldHaveUpdatedProject()
        {
            var projects = await ReadProjectsAsync(_oc.UserId);
            var projectNames = projects.Select(p => p.Name);
            Assert.That(projectNames, Is.EqualTo(new[] { "Project Alpha Updated" }));
        }
    }
}
