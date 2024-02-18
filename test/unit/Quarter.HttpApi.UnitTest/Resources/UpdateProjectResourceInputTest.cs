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
            yield return new object[] { new UpdateProjectResourceInput { name = "OK", description = "OK" } };
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
}
