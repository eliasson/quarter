using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Resources;

[TestFixture]
public class CreateProjectResourceInputTest
{
    [TestFixture]
    public class WithValidInput
    {
        public static IEnumerable<object[]> ValidResources()
        {
            yield return new object[] { new CreateProjectResourceInput { name = "OK", description = "OK" } };
        }

        [TestCaseSource(nameof(ValidResources))]
        public void ItShouldBeValid(CreateProjectResourceInput input)
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
    public class WithInvalidInput
    {
        public static IEnumerable<object[]> InvalidResources()
        {
            yield return new object[] { new CreateProjectResourceInput { name = null! }, "The name field is required." };
            yield return new object[] { new CreateProjectResourceInput { name = "" }, "The name field is required." };
            yield return new object[] { new CreateProjectResourceInput { description = null! }, "The description field is required." };
            yield return new object[] { new CreateProjectResourceInput { description = "" }, "The description field is required." };
        }

        [TestCaseSource(nameof(InvalidResources))]
        public void ItShouldNotBeValid(CreateProjectResourceInput input, string expectedError)
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