using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Queries;
using Quarter.Core.Utils;
using Quarter.Pages.Application.Home;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Application.Home;

[TestFixture]
public class CurrentWeekWidgetTest
{
    [TestFixture]
    public class Default : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            SetupTimesheets(1, 2, 3, 4, 5, 6, 7);
            RenderWithParameters(pb =>
                pb.Add(ps => ps.Summary, SummaryQueryResult));
        }

        [Test]
        public void ItShouldHaveTitle()
            => Assert.That(Title(), Is.EqualTo("Your current week"));

        [Test]
        public void ItShouldRenderAllDaysOfWeek()
        {
            Assert.That(LongDayNames(), Is.EqualTo(new []
            {
                "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
            }));
        }

        [Test]
        public void ItShouldRenderAllDaysOfWeekWithShortName()
        {
            Assert.That(ShortDayNames(), Is.EqualTo(new []
            {
                "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"
            }));
        }

        [Test]
        public void ItShouldRenderTotalHoursForAllTimesheets()
        {
            Assert.That(TotalHours(), Is.EqualTo(new [] { "0.25", "0.50", "0.75", "1.00", "1.25", "1.50", "1.75" }));
        }

        [Test]
        public void ItShouldUseSameUnitForAllHours()
        {
            var units = Units().ToHashSet();
            Assert.That(units, Is.EqualTo(new [] { "hours" }));
        }
    }

    public abstract class TestCase : BlazorComponentTestCase<CurrentWeekWidget>
    {
        protected TimesheetSummaryQueryResult SummaryQueryResult;

        protected void SetupTimesheets(params int[] quartersPerDay)
        {
            var startOfWeek = Date.Today().StartOfWeek().DateTime;
            var dataToRegister = quartersPerDay.Zip(Date.Sequence(new Date(startOfWeek), quartersPerDay.Length));
            var result = new TimesheetSummaryQueryResult();

            foreach (var data in dataToRegister)
            {
                var ts = Timesheet.CreateForDate(data.Second);
                ts.Register(new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), 0, data.First));
                result.Add(ts);
            }

            SummaryQueryResult = result;
        }

        protected string Title()
            => ComponentByTestAttribute("widget-title")?.TextContent;

        protected IEnumerable<string> LongDayNames()
            => ComponentsByTestAttribute("timesheet-day-long").Select(e => e.TextContent);

        protected IEnumerable<string> ShortDayNames()
            => ComponentsByTestAttribute("timesheet-day-short").Select(e => e.TextContent);

        protected IEnumerable<string> TotalHours()
            => ComponentsByTestAttribute("timesheet-total").Select(e => e.TextContent);

        protected IEnumerable<string> Units()
            => ComponentsByTestAttribute("timesheet-unit").Select(e => e.TextContent);
    }
}