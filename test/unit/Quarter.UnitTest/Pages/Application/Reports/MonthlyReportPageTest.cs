using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Css.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Quarter.Components;
using Quarter.Core.Models;
using Quarter.Core.Queries;
using Quarter.Core.Repositories;
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

    [TestFixture]
    public class WhenTimeIsRegistered : TestCase
    {
        private static readonly IdOf<Project> ProjectIdAlpha = IdOf<Project>.Random();
        private static readonly IdOf<Project> ProjectIdBravo = IdOf<Project>.Random();
        private static readonly IdOf<Activity> AlphaActivityIdOne = IdOf<Activity>.Random();
        private static readonly IdOf<Activity> AlphaActivityIdTwo = IdOf<Activity>.Random();
        private static readonly IdOf<Activity> BravoActivityIdOne = IdOf<Activity>.Random();
        private static readonly Date FromDate = new Date(DateTime.Parse("2022-03-01T00:00:00Z"));
        private static readonly Date ToDate = new Date(DateTime.Parse("2022-03-31T00:00:00Z"));

        [OneTimeSetUp]
        public void Setup()
        {
            //
            // Setup the projects and activities that are referenced from the result
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

            var usage = new Dictionary<Date, IList<ProjectTotalUsage>>
            {
                {FromDate, new List<ProjectTotalUsage>
                    {
                        new (ProjectIdAlpha,
                        10 * 15,
                        new List<ActivityUsage>()
                        {
                            new (AlphaActivityIdOne, 6 * 15, UtcDateTime.MinValue),
                            new (AlphaActivityIdTwo, 4 * 15, UtcDateTime.MinValue),
                        }, UtcDateTime.MinValue),
                    }
                },
                {ToDate, new List<ProjectTotalUsage>
                    {
                        new (ProjectIdAlpha,
                            10 * 15,
                            new List<ActivityUsage>()
                            {
                                new (AlphaActivityIdOne, 6 * 15, UtcDateTime.MinValue),
                                new (AlphaActivityIdTwo, 4 * 15, UtcDateTime.MinValue),
                            }, UtcDateTime.MinValue),
                        new (ProjectIdBravo, 20 * 15,
                        new List<ActivityUsage>()
                        {
                            new (BravoActivityIdOne, 20 * 15, UtcDateTime.MinValue),
                        }, UtcDateTime.MinValue)
                    }
                },
            };
            var result = new UsageOverTime(
                FromDate,
                ToDate,
                120,
                usage);

            RenderWithResult(result);
        }

        [Test]
        public void ItShouldRenderProjectRows()
            => Assert.That(ProjectRows(), Is.EqualTo(new[]
            {
                ("2022-03-01", "Alpha", "2.50"),
                ("2022-03-31", "Alpha", "2.50"),
                ("2022-03-31", "Bravo", "5.00"),
            }));

        [Test]
        public void ItShouldRenderActivityRows()
        => Assert.That(ActivityRows(), Is.EqualTo(new[]
            {
                ("Alpha One", "rgba(170, 170, 170, 1)", "rgba(187, 187, 187, 1)", "1.50"),
                ("Alpha Two", "rgba(204, 204, 204, 1)", "rgba(221, 221, 221, 1)", "1.00"),
                ("Alpha One", "rgba(170, 170, 170, 1)", "rgba(187, 187, 187, 1)", "1.50"),
                ("Alpha Two", "rgba(204, 204, 204, 1)", "rgba(221, 221, 221, 1)", "1.00"),
                ("Bravo One", "rgba(238, 238, 238, 1)", "rgba(255, 255, 255, 1)", "5.00"),
            }));
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

        protected void GoToPreviousMonth()
            => Component?.FindAll("[test=action-button]").First().Click();

        protected void GoToNextMonth()
            => Component?.FindAll("[test=action-button]").Last().Click();

        protected string LastNavigatedTo()
            =>_testNavigationManager.LastNavigatedTo();

        protected IEnumerable<(string date, string name, string total)> ProjectRows()
        {
            return Component?.FindAll("[test=report-project-row]").Select(elm =>
            {
                var date = elm.QuerySelector("[test=row-date]")?.TextContent;
                var name = elm.QuerySelector("[test=project-name]")?.TextContent;
                var total = elm.QuerySelector("[test=project-total]")?.TextContent;

                return (date, name, total);
            });
        }

        protected IEnumerable<(string name, string bgColor, string borderColor,  string total)> ActivityRows()
        {
            return Component?.FindAll("[test=report-activity-row]").Select(elm =>
            {
                var name = elm.QuerySelector("[test=activity-name]")?.TextContent;
                var bgColor = elm.QuerySelector("[test=activity-item-marker]").GetStyle()["background-color"];
                var borderColor = elm.QuerySelector("[test=activity-item-marker]").GetStyle()["border-color"];
                var total = elm.QuerySelector("[test=activity-total]")?.TextContent;
                return (name, bgColor, borderColor, total);
            });
        }
    }
}