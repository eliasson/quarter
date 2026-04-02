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

    [TestFixture]
    public class WhenUpdatingColor : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private ProjectResourceOutput? _output;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var project = await AddProject(_oc.UserId, "Project Beta");
            var input = new UpdateProjectResourceInput
            {
                color = "#e63946"
            };
            _output = await ApiService.UpdateProjectAsync(project.Id, input, _oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldReturnUpdatedColor()
            => Assert.That(_output?.color, Is.EqualTo("#E63946"));

        [Test]
        public async Task ItShouldHavePersistedUpdatedColor()
        {
            var projects = await ReadProjectsAsync(_oc.UserId);
            var project = projects.Single();
            Assert.That(project.Color, Is.EqualTo(Color.FromHexString("#e63946")));
        }
    }

    [TestFixture]
    public class WhenColorIsOmitted : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private ProjectResourceOutput? _output;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var project = await AddProject(_oc.UserId, "Project Gamma");
            var input = new UpdateProjectResourceInput
            {
                name = "Project Gamma Updated"
            };
            _output = await ApiService.UpdateProjectAsync(project.Id, input, _oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldReturnOriginalColor()
            => Assert.That(_output?.color, Is.EqualTo("#457B9D"));

        [Test]
        public async Task ItShouldNotHaveChangedColor()
        {
            var projects = await ReadProjectsAsync(_oc.UserId);
            var project = projects.Single();
            Assert.That(project.Color, Is.EqualTo(Color.FromHexString("#457b9d")));
        }
    }

    [TestFixture]
    public class WhenArchiving : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();

        [OneTimeSetUp]
        public async Task Setup()
        {
            var project = await AddProject(_oc.UserId, "Project alpha");

            var input = new UpdateProjectResourceInput
            {
                isArchived = true,
            };

            _ = await ApiService.UpdateProjectAsync(project.Id, input, _oc, CancellationToken.None);
        }

        [Test]
        public async Task ItShouldOnlyHaveUpdatedArchivedFlag()
        {
            var projects = await ReadProjectsAsync(_oc.UserId);
            var project = projects.Single();

            Assert.That(project.IsArchived, Is.True);
        }
    }
}
