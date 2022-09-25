using System;
using System.Collections.Generic;
using System.Linq;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Quarter.Components;
using Quarter.Core.Queries;
using Quarter.Core.Repositories;
using Quarter.Core.Utils;
using Quarter.Pages.Application.Reports;
using Quarter.Services;
using Quarter.State;
using Quarter.UnitTest.TestUtils;
using Quarter.Utils;
using TestContext = Bunit.TestContext;

namespace Quarter.UnitTest.Pages.Application.Reports;

[TestFixture]
public class MonthlyReportPageTest
{
    public class WhenUrlParameterIsMissing : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
            => Render();

        [Test]
        public void ItShouldSetTheCurrentTimesheetDate()
            => Assert.That(SelectedDate(), Is.EqualTo(DateTime.UtcNow.Date));
    }

    [TestFixture]
    public class WhenRenderedEmpty : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
            => RenderWithEmptyResult(DateTime.Parse("2022-03-17T00:00:00Z"),
                new Date(DateTime.Parse("2022-03-01T00:00:00Z")),
                new Date(DateTime.Parse("2022-03-31T00:00:00Z")));

        [Test]
        public void ItShouldDispatchLoadProjectsAction()
            => Assert.That(DidDispatchAction(new LoadProjects()), Is.True);

        [Test]
        public void ItShouldSetTheGivenDate()
            => Assert.That(SelectedDate(), Is.EqualTo(DateTime.Parse("2022-03-17T00:00:00Z")));

        [Test]
        public void ItHasTabs()
        {
            var context = Component?.FindComponent<PageContext>();

            Assert.That(context?.Instance.Tabs, Is.EquivalentTo(new[]
                {
                    new TabData("Weekly", Page.WeeklyReport),
                    new TabData("Monthly", Page.MonthlyReport),
                }
            ));
        }

        [Test]
        public void ItShouldRenderTheGivenMonth()
            => Assert.That(ReportTitle(), Is.EqualTo("March 2022"));

        [Test]
        public void ItShouldRenderTheStartAndAEndDates()
            => Assert.That(StartAndEndDate(), Is.EqualTo("2022-03-01 - 2022-03-31"));

        [Test]
        public void ItShouldRenderTotalHours()
            => Assert.That(TotalHours(), Is.EqualTo("0.00"));

        [Test]
        public void ItShouldShowEmptyCollectionMessage()
        {
            var emptyComponent = EmptyMessage();

            Assert.Multiple(() =>
            {
                Assert.That(emptyComponent?.Header, Is.EqualTo("No registered time"));
                Assert.That(emptyComponent?.Message, Is.EqualTo("There are no registered time for this month."));
            });
        }

        [Test]
        public void ItShouldNavigateToFirstDayInPreviousMonth()
        {
            GoToPreviousMonth();
            Assert.That(LastNavigatedTo, Is.EqualTo("/app/reports/month/2022-02-01"));
        }

        [Test]
        public void ItShouldNavigateToFirstDayInNextMonth()
        {
            GoToNextMonth();
            Assert.That(LastNavigatedTo, Is.EqualTo("/app/reports/month/2022-04-01"));
        }
    }

    public class TestCase : BlazorComponentTestCase<MonthlyReportPage>
    {
        private readonly TestQueryHandler _queryHandler = new ();
        private readonly IUserAuthorizationService _authService = new TestIUserAuthorizationService();
        private readonly TestNavigationManager _testNavigationManager = new ();

        protected override void ConfigureTestContext(TestContext ctx)
        {
            base.ConfigureTestContext(ctx);
            Context.Services.AddSingleton<IQueryHandler>(_queryHandler);
            Context.Services.AddSingleton(_authService);
            Context.Services.AddSingleton<NavigationManager>(_testNavigationManager);
        }

        protected void RenderWithEmptyResult(DateTime dateInTest, Date startOfMonth, Date endOfMonth)
        {
            _queryHandler.FakeMonthlyReportResult = new UsageOverTime(startOfMonth, endOfMonth, 0, new Dictionary<Date, IList<ProjectTotalUsage>>());
            RenderWithParameters(pb =>
                pb.Add(ps => ps.SelectedDate, dateInTest));
        }

        protected void RenderWithResult(UsageOverTime result)
        {
            _queryHandler.FakeMonthlyReportResult = result;
            Render();
        }

        protected DateTime? SelectedDate()
            => Component?.Instance.SelectedDate;

        protected string ReportTitle()
            => ComponentByTestAttribute("report-title").TextContent;

        protected string StartAndEndDate()
            => ComponentByTestAttribute("report-sub-title").TextContent;

        protected string TotalHours()
            => ComponentByTestAttribute("report-total-hours").TextContent;

        protected EmptyCollectionMessage EmptyMessage()
            => Component?.FindComponent<EmptyCollectionMessage>().Instance;

        protected IEnumerable<string> HeaderColumns()
            => Component?.FindAll(".qa-report-table thead th").Select(elm => elm.TextContent);

        protected void GoToPreviousMonth()
            => Component?.FindAll("[test=action-button]").First().Click();

        protected void GoToNextMonth()
            => Component?.FindAll("[test=action-button]").Last().Click();

        protected string LastNavigatedTo()
            =>_testNavigationManager.LastNavigatedTo();
    }
}