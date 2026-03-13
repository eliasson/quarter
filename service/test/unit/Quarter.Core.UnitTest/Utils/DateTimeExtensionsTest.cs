using System;
using System.Linq;
using NUnit.Framework;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Utils;

[TestFixture]
public class DateTimeExtensionsTest
{
    [TestCase("2020-12-12", "2020-11-01", true)]
    [TestCase("2020-12-12", "2021-01-12", true)]
    [TestCase("2020-12-12", "2020-10-12", false)]
    [TestCase("2020-12-12", "2019-11-01", false)]
    [TestCase("2020-12-12", "2022-01-12", false)]
    public void IsAdjacentMonth(string first, string second, bool expectedAdjacent)
    {
        var firstDate = DateTime.Parse($"{first}T00:00:00Z");
        var secondDate = DateTime.Parse($"{second}T00:00:00Z");

        Assert.Multiple(() =>
        {
            Assert.That(firstDate.IsAdjacentMonth(secondDate), Is.EqualTo(expectedAdjacent));
            Assert.That(secondDate.IsAdjacentMonth(firstDate), Is.EqualTo(expectedAdjacent));
        });
    }

    [TestCase("2020-12-31", 53)]
    [TestCase("2021-01-03", 53)]
    [TestCase("2021-05-29", 21)]
    [TestCase("2021-12-31", 52)]
    [TestCase("2022-01-03", 1)]
    public void ItShouldGiveWeekNumberForDate(string date, int expectedWeek)
    {
        var d = DateTime.Parse($"{date}T00:00:00Z");
        Assert.That(d.Iso8601WeekNumber(), Is.EqualTo(expectedWeek));
    }

    [TestCase("2020-12-31", "December 2020")]
    [TestCase("2022-01-03", "January 2022")]
    public void ItShouldGiveMonthAndYear(string date, string expected)
    {
        var d = DateTime.Parse($"{date}T00:00:00Z");
        Assert.That(d.MonthAndYear(), Is.EqualTo(expected));
    }

    [TestCase(2020, 12, 31)]
    [TestCase(2026, 2, 28)]
    [TestCase(2028, 2, 29)]
    public void ItShouldLastDayOfMonth(int year, int month, int expectedDay)
    {
        var d = new DateTime(year, month, 1);
        var expected = new DateTime(year, month, expectedDay);

        Assert.That(d.LastDayOfMonth(), Is.EqualTo(expected));
    }

    [Test]
    public void ItShouldGetTheRangeOfDateInBetween()
    {
        var first = new DateTime(2026, 2, 1);
        var last = new DateTime(2026, 2, 4);

        var range = first.RangeTo(last).Select(dt => dt.IsoString());
        Assert.That(range, Is.EqualTo(new []
        {
            "2026-02-01",
            "2026-02-02",
            "2026-02-03",
            "2026-02-04",
        }));
    }

    [Test]
    public void ItShouldGetSingleRangeOfDateForSameDate()
    {
        var d = DateTime.UtcNow;

        var range = d.RangeTo(d).Select(dt => dt.IsoString());
        Assert.That(range, Is.EqualTo(new [] { d.IsoString() }));
    }

    [Test]
    public void ItShouldThrowGettingRangeOfDateForOlderDate()
    {
        var today = DateTime.UtcNow;
        var yesterday = today.AddDays(-1);

        Assert.Throws<ArgumentException>(() => _ = today.RangeTo(yesterday).ToList());

    }
}
