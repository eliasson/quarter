using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Resources;

[TestFixture]
public class TimesheetResourceInputTest
{
    [TestFixture]
    public class WhenConstructingConstructingFromValidInput
    {
        public static IEnumerable<object[]> ValidResources()
        {
            yield return new object[] { new TimesheetResourceInput { date = "2023-01-28" } };
        }

        [TestCaseSource(nameof(ValidResources))]
        public void ItShouldBeValid(TimesheetResourceInput input)
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
    public class WhenConstructingConstructingFromInvalidInput
    {
        public static IEnumerable<object[]> InvalidResources()
        {
            yield return new object[] { new TimesheetResourceInput { date = null! }, "The date field is required." };
            yield return new object[] { new TimesheetResourceInput { date = "" }, "The date field is required." };
            yield return new object[] { new TimesheetResourceInput { date = "28-01-23" }, "The date must be given in ISO-8601 (YYYY-MM-DD)." };
        }

        [TestCaseSource(nameof(InvalidResources))]
        public void ItShouldNotBeValid(TimesheetResourceInput input, string expectedError)
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