using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Resources;

[TestFixture]
public class ProjectResourceTest
{
    public class WhenConstructingMinimalProjectOutput
    {
        private Project? _project;
        private ProjectResourceOutput? _output;

        [OneTimeSetUp]
        public void Setup()
        {
            _project = new Project("Project name", "Project description");
            _output = ProjectResourceOutput.From(_project);
        }

        [Test]
        public void ItShouldMapId()
            => Assert.That(_output?.id, Is.EqualTo(_project?.Id.Id.ToString()));

        [Test]
        public void ItShouldMapName()
            => Assert.That(_output?.name, Is.EqualTo("Project name"));
        [Test]
        public void ItShouldMapDescription()
            => Assert.That(_output?.description, Is.EqualTo("Project description"));
    }
}