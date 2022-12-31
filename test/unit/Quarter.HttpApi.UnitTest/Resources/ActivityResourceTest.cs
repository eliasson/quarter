using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Resources;

[TestFixture]
public class ActivityResourceTest
{
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

    [TestFixture]
    public class WhenConstructingActivityFromMinimalActivityInput : TestCase
    {
        private Activity? _activity;

        [OneTimeSetUp]
        public void Setup()
        {
            var input = new CreateActivityResourceInput
            {
                name = "Activity name",
                description = "Activity description",
                color = "#112233",
            };
            _activity = input.ToActivity(ProjectId);
        }

        [Test]
        public void ItShouldMapName()
            => Assert.That(_activity?.Name, Is.EqualTo("Activity name"));

        [Test]
        public void ItShouldMapDescription()
            => Assert.That(_activity?.Description, Is.EqualTo("Activity description"));

        [Test]
        public void ItShouldMapColor()
            => Assert.That(_activity?.Color.ToHex(), Is.EqualTo("#112233"));
    }

    [TestFixture]
    public class WhenConstructingActivityFromValidActivityInput : TestCase
    {
        public static IEnumerable<object[]> ValidResources()
        {
            yield return new object[] { new CreateActivityResourceInput { name = "OK", description = "OK", color = "#04a85b" } };
            yield return new object[] { new CreateActivityResourceInput { name = "OK", description = "OK", color = "#aabbcc" } };
            yield return new object[] { new CreateActivityResourceInput { name = "OK", description = "OK", color = "#abc" } };
        }

        [TestCaseSource(nameof(ValidResources))]
        public void ItShouldBeValid(CreateActivityResourceInput input)
        {
            var result = ObjectValidator.IsValid(input, out var errors);
            var errorMessages = errors.Select(_ => _.ErrorMessage);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(errorMessages, Is.Empty);
            });
        }
    }

    [TestFixture]
    public class WhenConstructingActivityFromInvalidActivityInput : TestCase
    {
        public static IEnumerable<object[]> InvalidResources()
        {
            yield return new object[] { new CreateActivityResourceInput { name = null! }, "The name field is required." };
            yield return new object[] { new CreateActivityResourceInput { name = "" }, "The name field is required." };
            yield return new object[] { new CreateActivityResourceInput { name = null! }, "The description field is required." };
            yield return new object[] { new CreateActivityResourceInput { description = "" }, "The description field is required." };
            yield return new object[] { new CreateActivityResourceInput { color = null! }, "The color field is required." };
            yield return new object[] { new CreateActivityResourceInput { color = "" }, "The color field is required." };
            yield return new object[] { new CreateActivityResourceInput { color = "yellow" }, "The color field is invalid, must be a HEX value (e.g. #04a85b)." };
        }

        [TestCaseSource(nameof(InvalidResources))]
        public void ItShouldNotBeValid(CreateActivityResourceInput input, string expectedError)
        {
            var result = ObjectValidator.IsValid(input, out var errors);
            var errorMessages = errors.Select(_ => _.ErrorMessage);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.False);
                Assert.That(errorMessages, Does.Contain(expectedError));
            });
        }
    }

    public class TestCase
    {
        protected readonly IdOf<Project> ProjectId = IdOf<Project>.Random();
        protected Activity? Activity;
        protected ActivityResourceOutput? Output;
    }
}