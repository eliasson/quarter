using NUnit.Framework;
using Quarter.Core.Models;

namespace Quarter.Core.UnitTest.Models
{
    public abstract class ProjectTest
    {
        public class WhenConstructed
        {
            private readonly Project _project = new Project("Alpha", "Alpha:Description");

            [Test]
            public void ItShouldHaveName()
                => Assert.That(_project.Name, Is.EqualTo("Alpha"));

            [Test]
            public void ItShouldHaveDescription()
                => Assert.That(_project.Description, Is.EqualTo("Alpha:Description"));

            [Test]
            public void ItShouldGetAnIdAssigned()
                => Assert.That(_project.Id, Is.Not.Null);

            [Test]
            public void ItShouldBeCreated()
                => Assert.That(_project.Created, Is.Not.Null);

            [Test]
            public void ItShouldNotBeUpdated()
                => Assert.That(_project.Updated, Is.Null);

            [Test]
            public void ItShouldNotBeArchived()
                => Assert.That(_project.IsArchived, Is.False);
        }

        public class WhenArchived
        {
            private readonly Project _project = new Project("Alpha", "Alpha:Description");

            [OneTimeSetUp]
            public void WhenArchivedSetUp()
            {
                _project.Archive();
            }

            [Test]
            public void ItShouldBeArchived()
                => Assert.That(_project.IsArchived, Is.True);
        }

        public class WhenRestored
        {
            private readonly Project _project = new Project("Alpha", "Alpha:Description");

            [OneTimeSetUp]
            public void WhenRestoredSetUp()
            {
                _project.Archive();
                _project.Restore();
            }

            [Test]
            public void ItShouldNotBeArchived()
                => Assert.That(_project.IsArchived, Is.False);
        }
    }
}