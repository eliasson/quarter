using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Css.Dom;
using Bunit;
using Bunit.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Quarter.Components;
using Quarter.Core.Models;
using Quarter.Core.Queries;
using Quarter.Core.Utils;
using Quarter.Pages.Application.Reports;
using Quarter.Services;
using Quarter.State;
using Quarter.State.ViewModels;
using Quarter.UnitTest.TestUtils;
using Quarter.Utils;
using TestContext = Bunit.TestContext;

namespace Quarter.UnitTest.Pages.Application.Reports;

[TestFixture]
public class WeeklyReportPageTest
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
                new Date(DateTime.Parse("2022-03-14T00:00:00Z")),
                new Date(DateTime.Parse("2022-03-20T00:00:00Z")));

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
                    new TabData("Monthly", Page.MonhtlyReport),
                }
            ));
        }

        [Test]
        public void ItShouldRenderTheGivenWeekNumber()
            => Assert.That(WeekNumber(), Is.EqualTo("Week 11"));

        [Test]
        public void ItShouldRenderTheStartAndAEndDates()
            => Assert.That(StartAndEndDate(), Is.EqualTo("2022-03-14 - 2022-03-20"));

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
                Assert.That(emptyComponent?.Message, Is.EqualTo("There are no registered time for this week."));
            });
        }

        [Test]
        public void ItShouldNavigateToFirstDayInPreviousWeek()
        {
            GoToPreviousWeek();
            Assert.That(LastNavigatedTo, Is.EqualTo("/app/reports/week/2022-03-07"));
        }

        [Test]
        public void ItShouldNavigateToFirstDayInNextWeek()
        {
            GoToNextWeek();
            Assert.That(LastNavigatedTo, Is.EqualTo("/app/reports/week/2022-03-21"));
        }
    }

    [TestFixture]
    public class WhenTimeIsRegistered : TestCase
    {
        private static readonly IdOf<Project> ProjectIdAlpha = IdOf<Project>.Random();
        private static readonly IdOf<Project> ProjectIdBravo = IdOf<Project>.Random();
        private static readonly IdOf<Activity> AlphaActivityIdOne = IdOf<Activity>.Random();
        private static readonly IdOf<Activity> AlphaActivityIdTwo = IdOf<Activity>.Random();
        private static readonly IdOf<Activity> BravoActivityIdOne = IdOf<Activity>.Random();

        [OneTimeSetUp]
        public void Setup()
        {

            var result = new WeeklyReportResult(
                new Date(DateTime.Parse("2022-03-14T00:00:00Z")),
                new Date(DateTime.Parse("2022-03-20T00:00:00Z")));

            result.AddOrUpdate(new ProjectSummary
            {
                Activities = new ActivitySummary[]
                {
                    new ()
                    {
                        ActivityId = AlphaActivityIdOne,
                        Duration = 2,
                    },
                    new ()
                    {
                        ActivityId = AlphaActivityIdTwo,
                        Duration = 4,
                    },
                },
                ProjectId = ProjectIdAlpha,
                Duration = 6
            }, 0); // Monday

            result.AddOrUpdate(new ProjectSummary
            {
                Activities = new ActivitySummary[]
                {
                    new ()
                    {
                        ActivityId = BravoActivityIdOne,
                        Duration = 96,
                    },
                },
                ProjectId = ProjectIdBravo,
                Duration = 96
            }, 6); // Friday

            StateManager.State.Projects.Add(new ProjectViewModel
            {
                Id = ProjectIdAlpha,
                Name = "Alpha",
                Activities = new List<ActivityViewModel>
                {
                    new ()
                    {
                        Id = AlphaActivityIdOne, Name = "Alpha One", Color = "#aaa", DarkerColor = "#bbb"
                    },
                    new ()
                    {
                        Id = AlphaActivityIdTwo, Name = "Alpha Two", Color = "#ccc", DarkerColor = "#ddd"
                    },
                }
            });
            StateManager.State.Projects.Add(new ProjectViewModel
            {
                Id = ProjectIdBravo,
                Name = "Bravo",
                Activities = new List<ActivityViewModel>
                {
                    new ()
                    {
                        Id = BravoActivityIdOne, Name = "Bravo One", Color = "#eee", DarkerColor = "#fff"
                    },
                }
            });

            RenderWithResult(result);
        }

        [Test]
        public void ItShouldRenderTotalHours()
            => Assert.That(TotalHours(), Is.EqualTo("25.50"));

        [Test]
        public void ItShouldNotRenderAnEmptyMessage()
            => Assert.Throws<ComponentNotFoundException>(() => EmptyMessage());

        [Test]
        public void ItShouldRenderHeaderForWeek()
            => Assert.That(HeaderColumns(), Is.EqualTo(new[]
            {
                "", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun", "Total"
            }));

        [Test]
        public void ItShouldRenderProjectRows()
            => Assert.That(ProjectRows(), Is.EqualTo(new[] { "Alpha", "Bravo" }));

        [Test]
        public void ItShouldRenderActivityRows()
            => Assert.That(ActivityRows(), Is.EqualTo(new[]
            {
                ("Alpha One", "rgba(170, 170, 170, 1)", "rgba(187, 187, 187, 1)", new [] {"0.50", "0.00", "0.00", "0.00", "0.00", "0.00", "0.00"}, "0.50"),
                ("Alpha Two", "rgba(204, 204, 204, 1)", "rgba(221, 221, 221, 1)", new [] {"1.00", "0.00", "0.00", "0.00", "0.00", "0.00", "0.00"}, "1.00"),
                ("Bravo One", "rgba(238, 238, 238, 1)", "rgba(255, 255, 255, 1)", new [] {"0.00", "0.00", "0.00", "0.00", "0.00", "0.00", "24.00"}, "24.00"),
            }));

        [Test]
        public void ItShouldRenderWeekdayTotals()
            => Assert.That(WeekdayTotals(), Is.EqualTo(new[]
            {
                "1.50", "0.00", "0.00", "0.00", "0.00", "0.00", "24.00",
            }));
    }

    public class TestCase : BlazorComponentTestCase<WeeklyReportPage>
    {
        private readonly TestQueryHandler _queryHandler = new TestQueryHandler();
        private readonly IUserAuthorizationService _authService = new TestIUserAuthorizationService();
        private readonly TestNavigationManager _testNavigationManager = new TestNavigationManager();

        protected override void ConfigureTestContext(TestContext ctx)
        {
            base.ConfigureTestContext(ctx);
            Context.Services.AddSingleton<IQueryHandler>(_queryHandler);
            Context.Services.AddSingleton(_authService);
            Context.Services.AddSingleton<NavigationManager>(_testNavigationManager);
        }

        protected void RenderWithEmptyResult(DateTime dateInTest, Date startOfWeek, Date endOfWeek)
        {
            _queryHandler.FakeWeeklyReportResult = new WeeklyReportResult(startOfWeek, endOfWeek);
            RenderWithParameters(pb =>
                pb.Add(ps => ps.SelectedDate, dateInTest));
        }

        protected void RenderWithResult(WeeklyReportResult result)
        {
            _queryHandler.FakeWeeklyReportResult = result;
            Render();
        }

        protected DateTime? SelectedDate()
            => Component?.Instance.SelectedDate;

        protected string WeekNumber()
            => ComponentByTestAttribute("report-title").TextContent;

        protected string StartAndEndDate()
            => ComponentByTestAttribute("report-sub-title").TextContent;

        protected string TotalHours()
            => ComponentByTestAttribute("report-total-hours").TextContent;

        protected EmptyCollectionMessage EmptyMessage()
            => Component?.FindComponent<EmptyCollectionMessage>().Instance;

        protected IEnumerable<string> HeaderColumns()
            => Component?.FindAll(".qa-report-table thead th").Select(elm => elm.TextContent);

        protected IEnumerable<string> ProjectRows()
            => Component?.FindAll("th[test=project-name]").Select(elm => elm.TextContent);

        protected IEnumerable<string> WeekdayTotals()
            => Component?.FindAll("td[test=report-weekday-total]").Select(elm => elm.TextContent);

        protected IEnumerable<(string name, string bgColor, string borderColor, string[] weekDays, string total)> ActivityRows()
        {
            return Component?.FindAll("[test=report-activity-row]").Select(elm =>
            {
                var name = elm.QuerySelector("[test=activity-name]")?.TextContent;
                var bgColor = elm.QuerySelector("[test=activity-item-marker]").GetStyle()["background-color"];
                var borderColor = elm.QuerySelector("[test=activity-item-marker]").GetStyle()["border-color"];
                var weekDays = elm.QuerySelectorAll("[test=report-activity-weekday]").Select(ielm => ielm.TextContent).ToArray();
                var total = elm.QuerySelector("[test=report-activity-total]")?.TextContent;
                return (name, bgColor, borderColor, weekDays, total);
            });
        }

        protected void GoToPreviousWeek()
            => Component?.FindAll("[test=action-button]").First().Click();

        protected void GoToNextWeek()
            => Component?.FindAll("[test=action-button]").Last().Click();

        protected string LastNavigatedTo()
            =>_testNavigationManager.LastNavigatedTo();
    }
}