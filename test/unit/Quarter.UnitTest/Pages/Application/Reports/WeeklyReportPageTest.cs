using System;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Quarter.Components;
using Quarter.Core.Queries;
using Quarter.Core.Utils;
using Quarter.Pages.Application.Reports;
using Quarter.Services;
using Quarter.UnitTest.TestUtils;
using Quarter.Utils;
using TestContext = Bunit.TestContext;

namespace Quarter.UnitTest.Pages.Application.Reports;

public abstract class WeeklyReportPageTest
{
    [TestFixture]
    public class WhenRenderedEmpty : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
            => RenderWithEmptyResult(
                new Date(DateTime.Parse("2022-03-14T00:00:00Z")),
                new Date(DateTime.Parse("2022-03-20T00:00:00Z")));

        [Test]
        public void ItHasAProjectTab()
        {
            var context = Component?.FindComponent<PageContext>();

            Assert.That(context?.Instance.Tabs, Is.EquivalentTo(new[]
                {
                    new TabData("Weekly", Page.Report)
                }
            ));
        }

        // TODO: Add test that today's date is used

        [Test]
        public void ItShouldRenderTheGivenWeekNumber()
            => Assert.That(WeekNumber(), Is.EqualTo("Week 11"));

        [Test]
        public void ItShouldRenderTheStartAndAEndDates()
            => Assert.That(StartAndEndDate(), Is.EqualTo("2022-03-14 - 2022-03-20"));
    }

    public class TestCase : BlazorComponentTestCase<WeeklyReportPage>
    {
        private readonly TestQueryHandler _queryHandler = new TestQueryHandler();
        private readonly IUserAuthorizationService _authService = new TestIUserAuthorizationService();

        protected override void ConfigureTestContext(TestContext ctx)
        {
            base.ConfigureTestContext(ctx);
            Context.Services.AddSingleton<IQueryHandler>(_queryHandler);
            Context.Services.AddSingleton(_authService);
        }

        protected void RenderWithEmptyResult(Date startOfWeek, Date endOfWeek)
        {
            _queryHandler.FakeWeeklyReportResult = new WeeklyReportResult(startOfWeek, endOfWeek);
                Render();
        }

        protected string WeekNumber()
            => ComponentByTestAttribute("report-title").TextContent;

        protected string StartAndEndDate()
            => ComponentByTestAttribute("report-sub-title").TextContent;
    }
}