using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Quarter.Core.Models;
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
            yield return new object[] { new TimesheetResourceInput { date = "2023-01-28", timeSlots = new [] { new TimeSlotInput
            {
                projectId = IdOf<Project>.Random().AsString(),
                activityId = IdOf<Activity>.Random().AsString(),
                offset = 0,
                duration = 1,
            } } } };
        }

        [TestCaseSource(nameof(ValidResources))]
        public void ItShouldBeValid(TimesheetResourceInput input)
        {
            var result = RecursiveObjectValidator.IsValid(input, out var errors);
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
            yield return new object[] { new TimesheetResourceInput { date = null }, "The date field is required." };
            yield return new object[] { new TimesheetResourceInput { date = "" }, "The date field is required." };
            yield return new object[] { new TimesheetResourceInput { date = "28-01-23" }, "The date must be given in ISO-8601 (YYYY-MM-DD)." };
            yield return new object[] { new TimesheetResourceInput { timeSlots = null }, "The timeSlots field is required." };
            yield return new object[] { new TimesheetResourceInput { timeSlots = Array.Empty<TimeSlotInput>() }, "The timeSlots field must not be empty." };
            yield return new object[] { new TimesheetResourceInput { date = "2023-01-28", timeSlots = new [] { new TimeSlotInput
            {
                activityId = IdOf<Activity>.Random().AsString(),
                offset = 0,
                duration = 1,
            } } }, "The projectId field is required." };
            yield return new object[] { new TimesheetResourceInput { date = "2023-01-28", timeSlots = new [] { new TimeSlotInput
            {
                projectId = IdOf<Project>.Random().AsString(),
                offset = 0,
                duration = 1,
            } } }, "The activityId field is required." };
            yield return new object[] { new TimesheetResourceInput { date = "2023-01-28", timeSlots = new [] { new TimeSlotInput
            {
                projectId = IdOf<Project>.Random().AsString(),
                activityId = IdOf<Activity>.Random().AsString(),
                offset = -1,
                duration = 1,
            } } }, "The field offset must be between 0 and 95." };
            yield return new object[] { new TimesheetResourceInput { date = "2023-01-28", timeSlots = new [] { new TimeSlotInput
            {
                projectId = IdOf<Project>.Random().AsString(),
                activityId = IdOf<Activity>.Random().AsString(),
                offset = 95,
                duration = 98,
            } } }, "The field duration must be between 1 and 96." };
        }

        [TestCaseSource(nameof(InvalidResources))]
        public void ItShouldNotBeValid(TimesheetResourceInput input, string expectedError)
        {
            var result = RecursiveObjectValidator.IsValid(input, out var errors);
            var errorMessages = errors.Select(_ => _.ErrorMessage);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.False);
                Assert.That(errorMessages, Does.Contain(expectedError));
            });
        }
    }
}