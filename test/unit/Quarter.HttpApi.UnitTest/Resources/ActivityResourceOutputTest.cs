using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Resources;

[TestFixture]
public class ActivityResourceOutputTest
{
    [TestFixture]
    public class WhenConstructingMinimalActivityOutput : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Activity = new Activity(ProjectId, "Activity name", "Activity description", Color.FromHexString("#112233"));
            Output = ActivityResourceOutput.From(Activity);
        }

        [Test]
        public void ItShouldMapId()
            => Assert.That(Output?.id, Is.EqualTo(Activity?.Id.Id.ToString()));

        [Test]
        public void ItShouldMapProjectId()
            => Assert.That(Output?.projectId, Is.EqualTo(Activity?.ProjectId.Id.ToString()));

        [Test]
        public void ItShouldMapName()
            => Assert.That(Output?.name, Is.EqualTo("Activity name"));

        [Test]
        public void ItShouldMapDescription()
            => Assert.That(Output?.description, Is.EqualTo("Activity description"));

        [Test]
        public void ItShouldMapColor()
            => Assert.That(Output?.color, Is.EqualTo("#112233"));

        [Test]
        public void ItShouldMapIsArchived()
            => Assert.That(Output?.isArchived, Is.False);

        [Test]
        public void ItShouldMapCreatedTimestamp()
            => Assert.That(Output?.created, Is.EqualTo(Activity?.Created.IsoString()));

        [Test]
        public void ItShouldLackUpdateTimestamp()
            => Assert.That(Output?.updated, Is.Null);
    }

    [TestFixture]
    public class WhenConstructingFullProjectOutput : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Activity = new Activity(ProjectId, "Activity name", "Activity description", Color.FromHexString("#112233"));
            Activity.Updated = UtcDateTime.Now();
            Activity.Archive();
            Output = ActivityResourceOutput.From(Activity);
        }

        [Test]
        public void ItShouldMapIsArchived()
            => Assert.That(Output?.isArchived, Is.True);

        [Test]
        public void ItShouldMapUpdatedTimestamp()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Output?.updated, Is.EqualTo(Activity?.Updated?.IsoString()));
                Assert.That(Output?.updated, Is.Not.Null);
            });
        }
    }

    public abstract class TestCase
    {
        protected readonly IdOf<Project> ProjectId = IdOf<Project>.Random();
        protected Activity? Activity;
        protected ActivityResourceOutput? Output;
    }
}
