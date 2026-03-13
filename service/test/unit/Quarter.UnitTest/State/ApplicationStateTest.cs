using System;
using NUnit.Framework;
using Quarter.State;

namespace Quarter.UnitTest.State;

[TestFixture]
public class ApplicationStateTest
{
    [Test]
    public void ItShouldThrowWhenSettingStartOfDayToLessThanZero()
    {
        var state = new ApplicationState();
        Assert.Throws<ArgumentException>(() => state.StartHourOfDay = -1);
    }

    [TestCase(12)]
    [TestCase(11)]
    public void ItShouldThrowWhenSettingStartOfDayToLessOrEqualThanEndOfDay(int sod)
    {
        var state = new ApplicationState
        {
            EndHourOfDay = 11
        };
        Assert.Throws<ArgumentException>(() => state.StartHourOfDay = sod);
    }

    [Test]
    public void ItShouldThrowWhenSettingEndOfDayToGreaterThan23()
    {
        var state = new ApplicationState();
        Assert.Throws<ArgumentException>(() => state.EndHourOfDay = 24);
    }

    [TestCase(12)]
    [TestCase(11)]
    public void ItShouldThrowWhenSettingEndOfDayToLessOrEqualThanStartOfDay(int eod)
    {
        var state = new ApplicationState
        {
            StartHourOfDay = 12
        };
        Assert.Throws<ArgumentException>(() => state.EndHourOfDay = eod);
    }
}
