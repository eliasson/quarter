using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Quarter.UnitTest
{
    public class PageTest
    {
        public static IEnumerable<object[]> PageTests()
        {
            yield return new object[] { Page.Home, "/app"};
            yield return new object[] { Page.Manage, "/app/manage/projects"};
            yield return new object[] { Page.Report, "/app/reports"};
            yield return new object[] { Page.Profile, "/app/profile"};
            yield return new object[] { Page.AdminUsers, "/admin/users"};
            yield return new object[] { Page.Admin, "/admin"};
            yield return new object[] { Page.Logout, "/account/logout"};
            yield return new object[] { Page.Login, "/account/login"};
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
            Assert.That(actual, Is.EqualTo($"/app/reports/{dt:yyyy-MM-dd}"));
        }
    }
}