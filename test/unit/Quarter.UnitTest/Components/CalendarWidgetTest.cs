using System;
using System.Linq;
using AngleSharp.Dom;
using Bunit;
using NUnit.Framework;
using Quarter.Components;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Components;

[TestFixture]
public abstract class CalendarWidgetTest
{
    public class WhenRendered : TestCase
    {
        private DateTime _selectedDate;

        [OneTimeSetUp]
        public void RenderComponent()
        {
            _selectedDate = DateTime.Parse("2020-12-13T00:00:00Z");
            RenderWithParameters(builder => builder
                .Add(ps => ps.SelectedDate, _selectedDate)
                .Add(ps => ps.LinkGeneratorFn, (dt) => $"/test/{dt:yyyy-MM-dd}"));
        }

        [Test]
        public void ItShouldRenderMonthHeader()
            => Assert.That(Month()?.TextContent, Is.EqualTo("December 2020"));

        [Test]
        public void ItShouldRenderDayHeaders()
            => Assert.That(DayHeaders(), Is.EqualTo(new [] { "Mo", "Tu", "We", "Th", "Fr", "Sa", "Su"}));

        [TestCase(0, "49", "30",  "1",  "2",  "3",  "4",  "5",  "6")]
        [TestCase(1, "50",  "7",  "8",  "9", "10", "11", "12", "13")]
        [TestCase(2, "51", "14", "15", "16", "17", "18", "19", "20")]
        [TestCase(3, "52", "21", "22", "23", "24", "25", "26", "27")]
        [TestCase(4, "53", "28", "29", "30", "31",  "1",  "2",  "3")]
        [TestCase(5,  "1",  "4",  "5",  "6",  "7",  "8",  "9", "10")]
        public void ItShouldRenderWeek(int calendarRow, string expectedWeek, params string[] expectedDays)
        {
            var row = WeekRows()[calendarRow];
            var weekNumber = row.QuerySelector("[test=calendar-week-number]").TextContent;
            var days = row.QuerySelectorAll("[test=calendar-day]").Select(d => d.TextContent);

            Assert.Multiple(() =>
            {
                Assert.That(weekNumber, Is.EqualTo(expectedWeek));
                Assert.That(days, Is.EqualTo(expectedDays));
            });
        }

        [Test]
        public void ItShouldHighlightSelectedDate()
            => Assert.That(SelectedDay()?.TextContent, Is.EqualTo("13"));

        [Test]
        public void ItShouldHighlightDatesForAdjacentMonths()
        {
            var days = AdjacentDays().Select(d => d.TextContent);
            Assert.That(days, Is.EqualTo(new [] {"30", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10"}));
        }

        [Test]
        public void ItShouldGenerateLinksForEachDay()
        {
            // Check the first link in each week row, this will assert adjacent months while not make the test too verbose
            var links = WeekRows().Select(wr => wr.QuerySelector("a").Attributes["href"].Value);
            Assert.That(links, Is.EqualTo(new []
            {
                "/test/2020-11-30",
                "/test/2020-12-07",
                "/test/2020-12-14",
                "/test/2020-12-21",
                "/test/2020-12-28",
                "/test/2021-01-04",
            }));
        }
    }

    public class WhenRenderingLeapYear : TestCase
    {
        private DateTime? _selectedDate;

        [OneTimeSetUp]
        public void RenderComponent()
        {
            _selectedDate = DateTime.Parse("2024-02-29T00:00:00Z");
            RenderWithParameters(builder =>
                builder.Add(ps => ps.SelectedDate, _selectedDate));
        }

        [Test]
        public void ItShouldRenderMonthHeader()
            => Assert.That(Month()?.TextContent, Is.EqualTo("February 2024"));

        [TestCase(0,  "5", "29", "30", "31",  "1",  "2",  "3",  "4")]
        [TestCase(1,  "6",  "5",  "6",  "7",  "8",  "9", "10", "11")]
        [TestCase(2,  "7", "12", "13", "14", "15", "16", "17", "18")]
        [TestCase(3,  "8", "19", "20", "21", "22", "23", "24", "25")]
        [TestCase(4,  "9", "26", "27", "28", "29",  "1",  "2",  "3")]
        [TestCase(5, "10",  "4",  "5",  "6",  "7",  "8",  "9", "10")]
        public void ItShouldRenderWeek(int calendarRow, string expectedWeek, params string[] expectedDays)
        {
            var row = WeekRows()[calendarRow];
            var weekNumber = row.QuerySelector("[test=calendar-week-number]").TextContent;
            var days = row.QuerySelectorAll("[test=calendar-day]").Select(d => d.TextContent);

            Assert.Multiple(() =>
            {
                Assert.That(weekNumber, Is.EqualTo(expectedWeek));
                Assert.That(days, Is.EqualTo(expectedDays));
            });
        }
    }

    public class WhenSelectingPreviousMonth : TestCase
    {
        private DateTime? _selectedDate;

        [OneTimeSetUp]
        public void RenderComponent()
        {
            _selectedDate = DateTime.Parse("2020-12-13T00:00:00Z");
            RenderWithParameters(builder => builder
                .Add(ps => ps.SelectedDate, _selectedDate)
                .Add(ps => ps.LinkGeneratorFn, (dt) => $"/test/{dt:yyyy-MM-dd}"));
            PreviousMonth()?.Click();
        }

        [Test]
        public void ItShouldRenderMonthHeader()
            => Assert.That(Month()?.TextContent, Is.EqualTo("November 2020"));

        [TestCase(0, "44", "26", "27", "28", "29", "30", "31",  "1")]
        [TestCase(1, "45",  "2",  "3",  "4",  "5",  "6",  "7",  "8")]
        [TestCase(2, "46",  "9", "10", "11", "12", "13", "14", "15")]
        [TestCase(3, "47", "16", "17", "18", "19", "20", "21", "22")]
        [TestCase(4, "48", "23", "24", "25", "26", "27", "28", "29")]
        [TestCase(5, "49", "30",  "1",  "2",  "3",  "4",  "5",  "6")]
        public void ItShouldRenderWeek(int calendarRow, string expectedWeek, params string[] expectedDays)
        {
            var row = WeekRows()[calendarRow];
            var weekNumber = row.QuerySelector("[test=calendar-week-number]").TextContent;
            var days = row.QuerySelectorAll("[test=calendar-day]").Select(d => d.TextContent);

            Assert.Multiple(() =>
            {
                Assert.That(weekNumber, Is.EqualTo(expectedWeek));
                Assert.That(days, Is.EqualTo(expectedDays));
            });
        }

        [Test]
        public void ItShouldNoLongerHighlightSelectedDate()
            => Assert.Catch<ElementNotFoundException>(() => SelectedDay());

        [Test]
        public void ItShouldHighlightDatesForAdjacentMonths()
        {
            var days = AdjacentDays().Select(d => d.TextContent);
            Assert.That(days, Is.EqualTo(new [] {"26", "27", "28", "29", "30", "31", "1", "2", "3", "4", "5", "6"}));
        }

        [Test]
        public void ItShouldNotChangeSelectedDate()
            => Assert.That(Component?.Instance.SelectedDate, Is.EqualTo(_selectedDate));
    }

    public class WhenSelectingNextMonth : TestCase
    {
        private DateTime? _selectedDate;

        [OneTimeSetUp]
        public void RenderComponent()
        {
            _selectedDate = DateTime.Parse("2020-12-13T00:00:00Z");
            RenderWithParameters(builder => builder
                .Add(ps => ps.SelectedDate, _selectedDate)
                .Add(ps => ps.LinkGeneratorFn, (dt) => $"/test/{dt:yyyy-MM-dd}"));
            NextMonth()?.Click();
        }

        [Test]
        public void ItShouldRenderMonthHeader()
            => Assert.That(Month()?.TextContent, Is.EqualTo("January 2021"));

        [TestCase(0, "53", "28", "29", "30", "31",  "1",  "2",  "3")]
        [TestCase(1,  "1",  "4",  "5",  "6",  "7",  "8",  "9", "10")]
        [TestCase(2,  "2", "11", "12", "13", "14", "15", "16", "17")]
        [TestCase(3,  "3", "18", "19", "20", "21", "22", "23", "24")]
        [TestCase(4,  "4", "25", "26", "27", "28", "29", "30", "31")]
        [TestCase(5,  "5",  "1",  "2",  "3",  "4",  "5",  "6",  "7")]
        public void ItShouldRenderWeek(int calendarRow, string expectedWeek, params string[] expectedDays)
        {
            var row = WeekRows()[calendarRow];
            var weekNumber = row.QuerySelector("[test=calendar-week-number]").TextContent;
            var days = row.QuerySelectorAll("[test=calendar-day]").Select(d => d.TextContent);

            Assert.Multiple(() =>
            {
                Assert.That(weekNumber, Is.EqualTo(expectedWeek));
                Assert.That(days, Is.EqualTo(expectedDays));
            });
        }

        [Test]
        public void ItShouldNoLongerHighlightSelectedDate()
            => Assert.Catch<ElementNotFoundException>(() => SelectedDay());

        [Test]
        public void ItShouldHighlightDatesForAdjacentMonths()
        {
            var days = AdjacentDays().Select(d => d.TextContent);
            Assert.That(days, Is.EqualTo(new [] { "28", "29", "30", "31", "1", "2", "3", "4", "5", "6", "7"}));
        }

        [Test]
        public void ItShouldNotChangeSelectedDate()
            => Assert.That(Component?.Instance.SelectedDate, Is.EqualTo(_selectedDate));
    }

    public class TestCase : BlazorComponentTestCase<CalendarWidget>
    {
        protected IElement Month()
            => Component?.Find("[test=calendar-month]");

        protected IElement[] WeekRows()
            => Component?.FindAll("[test=calendar-week]").ToArray()
               ?? Array.Empty<IElement>();

        protected string[] DayHeaders()
            => Component?.FindAll("[test=calendar-day-name]").Select(e => e.TextContent).ToArray()
               ?? Array.Empty<string>();

        protected IElement SelectedDay()
            => Component?.Find(".qa-calendar-selected");

        protected IElement[] AdjacentDays()
            => Component?.FindAll(".qa-calendar-adjacent").ToArray()
               ?? Array.Empty<IElement>();

        protected IElement PreviousMonth()
            => Component?.Find("[test=calendar-previous-month]");

        protected IElement NextMonth()
            => Component?.Find("[test=calendar-next-month]");
    }
}