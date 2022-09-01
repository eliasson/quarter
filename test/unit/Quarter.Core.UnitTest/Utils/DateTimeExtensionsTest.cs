using System;
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
}