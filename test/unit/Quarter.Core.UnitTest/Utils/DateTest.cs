using System;
using System.Collections.Generic;
using NUnit.Framework;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Utils;

[TestFixture]
public class DateTest
{
    [Test]
    public void ItShouldBeInUtc()
    {
        var date = Date.Today();

        Assert.That(date.DateTime.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public void ItShouldUseTimeZero()
    {
        var date = FromDateString("2021-08-10T12:34:56Z");

        Assert.That(date.DateTime.TimeOfDay, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void ItShouldSetYear()
    {
        var date = FromDateString("2021-08-10T12:34:56Z");

        Assert.That(date.DateTime.Year, Is.EqualTo(2021));
    }

    [Test]
    public void ItShouldSetMonth()
    {
        var date = FromDateString("2021-08-10T12:34:56Z");

        Assert.That(date.DateTime.Month, Is.EqualTo(08));
    }

    [Test]
    public void ItShouldSetDay()
    {
        var date = FromDateString("2021-08-10T12:34:56Z");

        Assert.That(date.DateTime.Day, Is.EqualTo(10));
    }

    [Test]
    public void ItShouldBeEqual()
    {
        var date1 = FromDateString("2021-08-10T12:34:56Z");
        var date2 = FromDateString("2021-08-10T10:20:30Z");

        Assert.That(date1, Is.EqualTo(date2));
    }

    [Test]
    public void ItShouldNotBeEqual()
    {
        var date1 = FromDateString("2021-08-11T10:20:30Z");
        var date2 = FromDateString("2021-08-12T10:20:30Z");

        Assert.That(date1, Is.Not.EqualTo(date2));
    }

    [Test]
    public void ItShouldReturnIsoString()
        => Assert.That(FromDateString("2021-08-01T10:20:30Z").IsoString(), Is.EqualTo("2021-08-01"));

    [Test]
    public void ItShouldGenerateRandomDates()
    {
        const int iterations = 10;
        var generated = new HashSet<string>();

        for (var i = 0; i < iterations; i++)
            generated.Add(Date.Random().IsoString());

        Assert.That(generated.Count, Is.EqualTo(iterations));
    }

    [Test]
    public void ItShouldReturnListOfSequentialDates()
    {
        var seq = Date.Sequence(FromDateString("2021-10-31T12:34:56Z"), 3);
        Assert.That(seq, Is.EqualTo(new[]
        {
            FromDateString("2021-10-31T12:34:56Z"),
            FromDateString("2021-11-01T00:00:00Z"),
            FromDateString("2021-11-02T00:00:00Z"),
        }));
    }

    [Test]
    public void ItShouldThrowIfCountIsLessThanOne()
        => Assert.Throws<ArgumentException>(() => Date.Sequence(Date.Today(), 0));

    [Test]
    public void ItShouldReturnListOfSequentialDatesBasedOnStartAndEndDate()
    {
        var seq = Date.Sequence(FromDateString("2021-10-31T12:34:56Z"), FromDateString("2021-11-02T00:00:00Z"));
        Assert.That(seq, Is.EqualTo(new[]
        {
            FromDateString("2021-10-31T12:34:56Z"),
            FromDateString("2021-11-01T00:00:00Z"),
            FromDateString("2021-11-02T00:00:00Z"),
        }));
    }

    [Test]
    public void ItShouldThrowIfEndDateIsNotGreaterThanStartDate()
        => Assert.Throws<ArgumentException>(() => Date.Sequence(Date.Today(), Date.Today()));

    [TestCase("2021-11-01T00:00:00Z", "2021-11-01T00:00:00Z")]
    [TestCase("2021-09-02T00:00:00Z", "2021-08-30T00:00:00Z")]
    [TestCase("2021-08-01T00:00:00Z", "2021-07-26T00:00:00Z")]
    public void ItShouldReturnStartOfWeek(string dateStr, string expectedDateStr)
    {
        var givenDate = FromDateString(dateStr);
        var startDate = givenDate.StartOfWeek();
        Assert.That(startDate, Is.EqualTo(FromDateString(expectedDateStr)));
    }

    [TestCase("2021-11-01T00:00:00Z", "2021-11-07T00:00:00Z")]
    [TestCase("2021-09-02T00:00:00Z", "2021-09-05T00:00:00Z")]
    [TestCase("2021-08-01T00:00:00Z", "2021-08-01T00:00:00Z")]
    public void ItShouldReturnEndOfWeek(string dateStr, string expectedDateStr)
    {
        var givenDate = FromDateString(dateStr);
        var endDate = givenDate.EndOfWeek();
        Assert.That(endDate, Is.EqualTo(FromDateString(expectedDateStr)));
    }

    [TestCase("2021-11-01T00:00:00Z", "2021-11-01T00:00:00Z")]
    [TestCase("2021-08-31T00:00:00Z", "2021-08-01T00:00:00Z")]
    [TestCase("2020-02-29T00:00:00Z", "2020-02-01T00:00:00Z")]
    public void ItShouldReturnStartOfMonth(string dateStr, string expectedDateStr)
    {
        var givenDate = FromDateString(dateStr);
        var startDate = givenDate.StartOfMonth();
        Assert.That(startDate, Is.EqualTo(FromDateString(expectedDateStr)));
    }

    [TestCase("2021-11-01T00:00:00Z", "2021-11-30T00:00:00Z")]
    [TestCase("2021-08-31T00:00:00Z", "2021-08-31T00:00:00Z")]
    [TestCase("2020-02-10T00:00:00Z", "2020-02-29T00:00:00Z")]
    [TestCase("2022-12-10T00:00:00Z", "2022-12-31T00:00:00Z")]
    public void ItShouldReturnEndOfMonth(string dateStr, string expectedDateStr)
    {
        var givenDate = FromDateString(dateStr);
        var endDate = givenDate.EndOfMonth();
        Assert.That(endDate, Is.EqualTo(FromDateString(expectedDateStr)));
    }

    private static Date FromDateString(string dateString)
        => new Date(DateTime.Parse(dateString));
}