using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Resources;

[TestFixture]
public class ProjectResourceOutputTest
{
    [TestFixture]
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

        [Test]
        public void ItShouldMapIsArchived()
            => Assert.That(_output?.isArchived, Is.False);

        [Test]
        public void ItShouldMapCreatedTimestamp()
            => Assert.That(_output?.created, Is.EqualTo(_project?.Created.IsoString()));

        [Test]
        public void ItShouldLackUpdateTimestamp()
            => Assert.That(_output?.updated, Is.Null);
    }

    [TestFixture]
    public class WhenConstructingFullProjectOutput
    {
        private Project? _project;
        private ProjectResourceOutput? _output;

        [OneTimeSetUp]
        public void Setup()
        {
            _project = new Project("Project name", "Project description");
            _project.Updated = UtcDateTime.Now();
            _project.Archive();
            _output = ProjectResourceOutput.From(_project);
        }

        [Test]
        public void ItShouldMapIsArchived()
            => Assert.That(_output?.isArchived, Is.True);

        [Test]
        public void ItShouldMapUpdatedTimestamp()
        {
            Assert.Multiple(() =>
            {
                Assert.That(_output?.updated, Is.EqualTo(_project?.Updated?.IsoString()));
                Assert.That(_output?.updated, Is.Not.Null);
            });
        }
    }
}
