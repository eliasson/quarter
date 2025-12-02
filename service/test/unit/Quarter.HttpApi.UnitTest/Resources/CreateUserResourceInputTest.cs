using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Quarter.Core.Utils;

namespace Quarter.HttpApi.UnitTest.Resources;

[TestFixture]
public class CreateUserResourceInputTest
{
    [TestFixture]
    public class WithValidInput
    {
        public static IEnumerable<object[]> ValidResources()
        {
            yield return [new CreateUserResourceInput { email = "jane@example.com" }];
        }

        [TestCaseSource(nameof(ValidResources))]
        public void ItShouldBeValid(CreateUserResourceInput input)
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
            yield return [new CreateUserResourceInput { email = null! }, "The email field is required."];
            yield return [new CreateUserResourceInput { email = "" }, "The email field is required."];
            yield return [new CreateUserResourceInput { email = "jane.example.com" }, "The email field is not a valid e-mail address."];
        }

        [TestCaseSource(nameof(InvalidResources))]
        public void ItShouldNotBeValid(CreateUserResourceInput input, string expectedError)
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
