using System;
using System.Globalization;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.Pages.Application.Home;
using Quarter.UnitTest.TestUtils;
using TestContext = Bunit.TestContext;

namespace Quarter.UnitTest.Pages.Application.Home;

[TestFixture]
public class TimesheetTotalWidgetTest
{
    public class WhenRendered : TestCase
    {
        private Timesheet _timesheet;
        private static readonly DateTime Today = DateTime.Today;

        [OneTimeSetUp]
        public void Setup()
        {
            _timesheet = Timesheet.CreateForDate(new Date(Today));

            RenderWithParameters(pb =>
            {
                pb.Add(ps => ps.Timesheet, _timesheet);
            });
        }

        [Test]
        public void ItShouldDisplayDayName()
        {
            var expectedDay = Today.ToString("dddd", CultureInfo.InvariantCulture);

            Assert.That(NameOfDay(), Is.EqualTo(expectedDay));
        }

        [Test]
        public void ItShouldDisplayDayDate()
        {
            var expectedDate = Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            Assert.That(DateOfDay(), Is.EqualTo(expectedDate));
        }

        [Test]
        public void ItShouldLinkToTimesheetPage()
        {
            Component?.Find(".qa-list-item").Click();
            var expectedDate = Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var expectedUrl = $"/app/timesheet/{expectedDate}";

            Assert.That(LastNavigatedTo, Is.EqualTo(expectedUrl));
        }
    }

    public abstract class TestCase : BlazorComponentTestCase<TimesheetTotalWidget>
    {
        private readonly TestNavigationManager _testNavigationManager = new TestNavigationManager();

        protected override void ConfigureTestContext(TestContext ctx)
        {
            base.ConfigureTestContext(ctx);
            ctx.Services.AddSingleton<NavigationManager>(_testNavigationManager);
        }

        protected string NameOfDay()
            => ComponentByTestAttribute("name-of-day")?.TextContent;

        protected string DateOfDay()
            => ComponentByTestAttribute("iso-day")?.TextContent;

        protected string LastNavigatedTo()
            =>_testNavigationManager.LastNavigatedTo();
    }
}