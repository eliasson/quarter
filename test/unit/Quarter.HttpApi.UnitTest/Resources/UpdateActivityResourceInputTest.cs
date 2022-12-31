using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Resources;

[TestFixture]
public class UpdateActivityResourceInputTest
{
    [TestFixture]
    public class WhenConstructingActivityFromValidActivityInput
    {
        public static IEnumerable<object[]> ValidResources()
        {
            yield return new object[] { new UpdateActivityResourceInput { name = "OK", description = "OK", color = "#04a85b" } };
            yield return new object[] { new UpdateActivityResourceInput { name = "OK" } };
            yield return new object[] { new UpdateActivityResourceInput { description = "OK" } };
        }

        [TestCaseSource(nameof(ValidResources))]
        public void ItShouldBeValid(UpdateActivityResourceInput input)
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
            yield return new object[] { new UpdateActivityResourceInput { color = "yellow" }, "The color field is invalid, must be a HEX value (e.g. #04a85b)." };
        }

        [TestCaseSource(nameof(InvalidResources))]
        public void ItShouldNotBeValid(UpdateActivityResourceInput input, string expectedError)
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