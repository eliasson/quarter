using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Quarter.UnitTest;

[TestFixture]
public class PageTest
{
    public static IEnumerable<object[]> PageTests()
    {
        yield return new object[] { Page.Home, "/app" };
        yield return new object[] { Page.Manage, "/app/manage/projects" };
        yield return new object[] { Page.WeeklyReport, "/app/reports/week" };
        yield return new object[] { Page.MonthlyReport, "/app/reports/month" };
        yield return new object[] { Page.Profile, "/app/profile" };
        yield return new object[] { Page.AdminUsers, "/admin/users" };
        yield return new object[] { Page.Admin, "/admin" };
        yield return new object[] { Page.Logout, "/account/logout" };
        yield return new object[] { Page.Login, "/account/login" };
    }

    [TestCaseSource(nameof(PageTests))]
    public void ItShouldResolveToUrl(string actual, string expected)
        => Assert.That(actual, Is.EqualTo(expected));

    [Test]
    public void ItShouldResolveToTimesheetUrl()
    {
        var dt = DateTime.UtcNow;
        var actual = Page.Timesheet(dt);
        Assert.That(actual, Is.EqualTo($"/app/timesheet/{dt:yyyy-MM-dd}"));
    }

    [Test]
    public void ItShouldResolveToWeekReportUrl()
    {
        var dt = DateTime.UtcNow;
        var actual = Page.WeekReport(dt);
        Assert.That(actual, Is.EqualTo($"/app/reports/week/{dt:yyyy-MM-dd}"));
    }

    [Test]
    public void ItShouldResolveToMonthlyReportUrl()
    {
        var dt = DateTime.UtcNow;
        var actual = Page.MonthReport(dt);
        Assert.That(actual, Is.EqualTo($"/app/reports/month/{dt:yyyy-MM-dd}"));
    }
}
