using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Resources;

[TestFixture]
public class UpdateProjectResourceInputTest
{
    [TestFixture]
    public class WithValidInput
    {
        public static IEnumerable<object[]> ValidResources()
        {
            yield return [new UpdateProjectResourceInput { name = "OK", description = "OK" }];
            yield return [new UpdateProjectResourceInput { name = "OK", isArchived = true}];
            yield return [new UpdateProjectResourceInput { name = "OK", color = "#457b9d" }];
            yield return [new UpdateProjectResourceInput { name = "OK", color = "#FFF" }];
        }

        [TestCaseSource(nameof(ValidResources))]
        public void ItShouldBeValid(UpdateProjectResourceInput input)
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
            yield return [new UpdateProjectResourceInput { name = "OK", color = "invalid" }, "The color field is invalid, must be a HEX value (e.g. #04a85b)."];
            yield return [new UpdateProjectResourceInput { name = "OK", color = "457b9d" }, "The color field is invalid, must be a HEX value (e.g. #04a85b)."];
        }

        [TestCaseSource(nameof(InvalidResources))]
        public void ItShouldNotBeValid(UpdateProjectResourceInput input, string expectedError)
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
