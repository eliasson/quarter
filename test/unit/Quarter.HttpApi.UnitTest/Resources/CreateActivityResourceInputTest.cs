using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Resources;

[TestFixture]
public class CreateActivityResourceInputTest
{
    [TestFixture]
    public class WhenConstructingActivityFromMinimalActivityInput
    {
        private readonly IdOf<Project> _projectId = IdOf<Project>.Random();
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
            _activity = input.ToActivity(_projectId);
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

        [Test]
        public void ItShouldSetProjectId()
            => Assert.That(_activity?.ProjectId, Is.EqualTo(_projectId));
    }

    [TestFixture]
    public class WhenConstructingActivityFromValidActivityInput
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
    public class WhenConstructingActivityFromInvalidActivityInput
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
}