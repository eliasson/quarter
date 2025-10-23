using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Models
{
    [TestFixture]
    public class ActivityTest
    {
        public class WhenConstructed
        {
            private static readonly IdOf<Project> ProjectId = IdOf<Project>.Random();
            private readonly Activity _activity = new(ProjectId, "Alpha", "Alpha:Description", Color.FromHexString("#112233"));

            [Test]
            public void ItShouldHaveProjectId()
                => Assert.That(_activity.ProjectId, Is.EqualTo(ProjectId));

            [Test]
            public void ItShouldHaveName()
                => Assert.That(_activity.Name, Is.EqualTo("Alpha"));

            [Test]
            public void ItShouldHaveDescription()
                => Assert.That(_activity.Description, Is.EqualTo("Alpha:Description"));

            [Test]
            public void ItShouldHaveColor()
                => Assert.That(_activity.Color, Is.EqualTo(Color.FromHexString("#112233")));

            [Test]
            public void ItShouldGetAnIdAssigned()
                => Assert.That(_activity.Id, Is.Not.Null);

            [Test]
            public void ItShouldBeCreated()
                => Assert.That(_activity.Created, Is.Not.Null);

            [Test]
            public void ItShouldNotBeUpdated()
                => Assert.That(_activity.Updated, Is.Null);

            [Test]
            public void ItShouldNotBeArchived()
                => Assert.That(_activity.IsArchived, Is.False);
        }
    }
}
