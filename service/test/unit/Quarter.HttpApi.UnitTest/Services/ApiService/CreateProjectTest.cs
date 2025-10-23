using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class CreateProjectTest
{
    [TestFixture]
    public class WhenInputIsValid : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private ProjectResourceOutput? _output;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var input = new CreateProjectResourceInput
            {
                name = "Project alpha",
                description = "Description alpha"
            };
            _output = await ApiService.CreateProjectAsync(input, _oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldReturnOutputResourceForProject()
            => Assert.That(_output?.name, Is.EqualTo("Project alpha"));

        [Test]
        public async Task ItShouldHaveCreatedProject()
        {
            var projects = await ReadProjectsAsync(_oc.UserId);
            var projectNames = projects.Select(p => p.Name);
            Assert.That(projectNames, Is.EqualTo(new[] { "Project alpha" }));
        }
    }
}
