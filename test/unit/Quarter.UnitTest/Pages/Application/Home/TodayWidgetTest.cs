using System;
using NUnit.Framework;
using Quarter.Pages.Application.Home;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Application.Home;

[TestFixture]
public class TodayWidgetTest
{
    public class Default : TestCase
    {
        private readonly DateTime _dtInTest = DateTime.Parse("2020-12-13T00:00:00Z");

        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb =>
            {
                pb.Add(ps => ps.CurrentDate, _dtInTest);
            });
        }

        [Test]
        public void ItShouldDisplayDayName()
            => Assert.That(NameOfDay(), Is.EqualTo("Sunday"));

        [Test]
        public void ItShouldDisplayDay()
            => Assert.That(Day(), Is.EqualTo("13"));

        [Test]
        public void ItShouldDisplayMonthName()
            => Assert.That(NameOfMonth(), Is.EqualTo("December"));
    }

    public abstract class TestCase : BlazorComponentTestCase<TodayWidget>
    {
        protected string NameOfDay()
            => ComponentByTestAttribute("name-of-day")?.TextContent;

        protected string Day()
            => ComponentByTestAttribute("day")?.TextContent;

        protected string NameOfMonth()
            => ComponentByTestAttribute("name-of-month")?.TextContent;
    }
}