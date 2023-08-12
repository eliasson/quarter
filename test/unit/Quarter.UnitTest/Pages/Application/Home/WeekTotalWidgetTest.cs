using NUnit.Framework;
using Quarter.Core.Queries;
using Quarter.Pages.Application.Home;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Application.Home;

[TestFixture]
public class WeekTotalWidgetTest
{
    public class Default : TestCase
    {
        private TimesheetSummaryQueryResult _summary;

        [OneTimeSetUp]
        public void Setup()
        {
            _summary = new TimesheetSummaryQueryResult { TotalMinutes = 90 };
            RenderWithParameters(pb =>
            {
                pb.Add(ps => ps.Summary, _summary);
            });
        }

        [Test]
        public void ItShouldHaveTitle()
            => Assert.That(Title(), Is.EqualTo("Total this week"));
        [Test]
        public void ItShouldDisplayTotal()
            => Assert.That(Total(), Is.EqualTo("1.50"));

        [Test]
        public void ItShouldDisplayUnit()
            => Assert.That(Unit(), Is.EqualTo("hours"));
    }

    public class TestCase : BlazorComponentTestCase<WeekTotalWidget>
    {
        protected string Title()
            => ComponentByTestAttribute("widget-title")?.TextContent;

        protected string Total()
            => ComponentByTestAttribute("week-total")?.TextContent;

        protected string Unit()
            => ComponentByTestAttribute("week-unit")?.TextContent;
    }
}