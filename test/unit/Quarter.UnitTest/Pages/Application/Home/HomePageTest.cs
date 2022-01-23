using System.Collections.Generic;
using System.Linq;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Queries;
using Quarter.Core.Utils;
using Quarter.Pages.Application.Home;
using Quarter.Services;
using Quarter.UnitTest.TestUtils;
using TestContext = Bunit.TestContext;

namespace Quarter.UnitTest.Pages.Application.Home;

public abstract class HomePageTest
{
    public class WhenNoTimeRegistered : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithEmptyResult();
        }

        [Test]
        public void ItShouldDisplayTodayWidget()
            => Assert.DoesNotThrow(() => Component?.FindComponent<TodayWidget>());
    }

    public class WheTimeIsRegistered : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            SetupTimesheets(1, 2, 3, 4, 5, 6, 7);
            Render();
        }

        [Test]
        public void ItShouldDisplayTodayWidget()
            => Assert.DoesNotThrow(() => Component?.FindComponent<TodayWidget>());

        [Test]
        public void ItShouldDisplayWeekTotalWidget()
        {
            var component = Component?.FindComponent<WeekTotalWidget>();
            Assert.That(component?.Instance?.Summary?.TotalMinutes,
                Is.EqualTo(1 * 15 + 2 * 15 + 3 * 15 + 4 * 15 + 5 * 15 + 6 * 15 + 7 * 15));
        }

        [Test]
        public void ItShouldDisplayTimesheetsForAllWeek()
            => Assert.That(TotalMinutes(), Is.EqualTo(new [] {
                1*15, 2*15, 3*15, 4*15, 5*15, 6*15, 7*15 }));
    }

    public abstract class TestCase : BlazorComponentTestCase<HomePage>
    {
        private readonly TestQueryHandler _queryHandler = new TestQueryHandler();
        private readonly IUserAuthorizationService _authService = new TestIUserAuthorizationService();

        protected override void ConfigureTestContext(TestContext ctx)
        {
            base.ConfigureTestContext(ctx);
            Context.Services.AddSingleton<IQueryHandler>(_queryHandler);
            Context.Services.AddSingleton(_authService);
        }

        protected void RenderWithEmptyResult()
        {
            _queryHandler.FakeTimesheetSummaryQueryResult = new TimesheetSummaryQueryResult();
            Render();
        }

        protected void SetupTimesheets(params int[] quartersPerDay)
        {
            var startOfWeek = Date.Today().StartOfWeek().DateTime;
            var dataToRegister = quartersPerDay.Zip(Date.Sequence(new Date(startOfWeek), quartersPerDay.Length));
            var result = new TimesheetSummaryQueryResult();

            foreach (var data in dataToRegister)
            {
                var ts = Timesheet.CreateForDate(data.Second);
                ts.Register(new ActivityTimeSlot(IdOf<Project>.Random(), IdOf<Activity>.Random(), 0, data.First));
                result.Add(ts);
            }

            _queryHandler.FakeTimesheetSummaryQueryResult = result;
        }

        protected IEnumerable<int?> TotalMinutes()
        {
            var totalComponents = Component?.FindComponents<TimesheetTotalWidget>();
            return totalComponents?
                .Select(c => c.Instance.Timesheet?.TotalMinutes());
        }
    }
}