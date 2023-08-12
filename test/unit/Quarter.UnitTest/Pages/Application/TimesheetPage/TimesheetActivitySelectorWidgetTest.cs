using System.Collections.Generic;
using System.Linq;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using Bunit;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Pages.Application.Timesheet;
using Quarter.State;
using Quarter.State.ViewModels;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Application.TimesheetPage;

[TestFixture]
public class TimesheetActivitySelectorWidgetTest
{
    public class WhenThereAreNoProjects : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
            => Render();

        [Test]
        public void ItShouldNotRenderAnyProjects()
            => Assert.That(ProjectItems(), Is.Empty);
    }

    public class WhenThereAreProjects : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            StateManager.State.Projects = ListOfProjects;
            Render();
        }

        [Test]
        public void ItShouldRenderAllProjectsThatHaveActivities()
        {
            var projectNames = ProjectItems()?.Select(p => p.TextContent);
            Assert.That(projectNames, Is.EqualTo(new[] { "Project One", "Project Two" }));
        }

        [Test]
        public void ItShouldRenderAllNonArchivedActivities()
        {
            var projectNames = ActivityTitles()?.Select(p => p.TextContent);
            // The first "Erase activity" is the small screen header (based on media queries)
            Assert.That(projectNames, Is.EqualTo(new[] { "Erase activity", "P1A", "P2A", "P2B", "Erase activity" }));
        }

        [Test]
        public void ItShouldRenderActivitiesUsingCustomStyles()
        {
            Assert.That(ActivityMarkers(), Is.EqualTo(new[]
            {
                ("P1A", "rgba(17, 17, 17, 1)", "rgba(34, 34, 34, 1)"),
                ("P2A", "rgba(51, 51, 51, 1)", "rgba(68, 68, 68, 1)"),
                ("P2B", "rgba(85, 85, 85, 1)", "rgba(102, 102, 102, 1)"),
            }));
        }

        [Test]
        public void ItShouldNotHaveAnyActiveActivity()
            => Assert.That(ActiveActivities(), Is.Empty);

        [Test]
        public void ItShouldHaveEraseActionActive()
            => Assert.That(EraseAction()?.ClassList.Contains("qa-is-active"), Is.True);
    }

    public class WhenSelectingErase : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            StateManager.State.Projects = ListOfProjects;
            Render();
            ActivityItem(1)?.Click();
            EraseAction()?.Click();
        }

        [Test]
        public void ItShouldDispatchAction()
            => Assert.True(DidDispatchAction(new SelectEraseActivityAction()));
    }

    public class WhenSelectingActivity : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            StateManager.State.Projects = ListOfProjects;
            Render();
            ActivityItem(1)?.Click();
        }

        [Test]
        public void ItShouldDispatchAction()
        {
            var selectedActivity = new SelectedActivity(ListOfProjects[1].Id, ListOfProjects[1].Activities[0].Id);
            Assert.True(DidDispatchAction(new SelectActivityAction(selectedActivity)));
        }
    }

    public class WhenThereIsASelectedActivity : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            StateManager.State.Projects = ListOfProjects;
            StateManager.State.SelectedActivity = new SelectedActivity(ListOfProjects[1].Id, ListOfProjects[1].Activities[0].Id);
            Render();
        }

        [Test]
        public void ItShouldActivateActivity()
        {
            var active = ActivityItem(1)?.ClassList.Contains("qa-is-active");
            Assert.That(active, Is.True);
        }

        [Test]
        public void ItShouldNotHaveEraseActionActive()
            => Assert.That(EraseAction()?.ClassList.Contains("qa-is-active"), Is.False);
    }

    public class TestCase : BlazorComponentTestCase<TimesheetActivitySelectorWidget>
    {
        protected IRefreshableElementCollection<IElement> ProjectItems()
            => Component?.FindAll("[test=project-item]");

        private IRefreshableElementCollection<IElement> ActivityItems()
            => Component?.FindAll("[test=activity-item]");

        protected IRefreshableElementCollection<IElement> ActiveActivities()
            => Component?.FindAll("li.qa-is-activity");

        protected IElement ActivityItem(int index)
        {
            var items = ActivityItems();
            return items?[index];
        }

        protected IEnumerable<IElement> ActivityTitles()
            => Component?.FindAll("[test=activity-item-title]");

        protected IElement EraseAction()
            => Component?.Find("[test=erase-item]");

        protected IEnumerable<(string title, string bgColor, string borderColor)> ActivityMarkers()
        {
            return Component?.FindAll("[test=activity-item]").Select(item =>
            {
                var title = item.QuerySelector("[test=activity-item-title]")?.TextContent;
                var bgColor = item.QuerySelector("[test=activity-item-marker--color]").GetStyle()["background-color"];
                var borderColor = item.QuerySelector("[test=activity-item-marker--color]").GetStyle()["border-color"];
                return (title, bgColor, borderColor);
            });
        }
    }

    private static readonly List<ProjectViewModel> ListOfProjects = new List<ProjectViewModel>
    {
        new ProjectViewModel
        {
            Id = IdOf<Project>.Random(),
            Name = "Project One",
            Activities = new List<ActivityViewModel>
            {
                new ActivityViewModel
                {
                    Id = IdOf<Activity>.Random(),
                    Name = "P1A",
                    Color = "#111111",
                    DarkerColor = "#222222",
                }
            }
        },
        new ProjectViewModel
        {
            Id = IdOf<Project>.Random(),
            Name = "Project Two",
            Activities = new List<ActivityViewModel>
            {
                new ActivityViewModel
                {
                    Id = IdOf<Activity>.Random(),
                    Name = "P2A",
                    Color = "#333333",
                    DarkerColor = "#444444",
                },
                new ActivityViewModel
                {
                    Id = IdOf<Activity>.Random(),
                    Name = "P2B",
                    Color = "#555555",
                    DarkerColor = "#666666",
                },
                new ActivityViewModel
                {
                    Id = IdOf<Activity>.Random(),
                    Name = "P2C",
                    Color = "#555555",
                    DarkerColor = "#666666",
                    IsArchived = true,
                }
            }
        },
        new ProjectViewModel
        {
            Id = IdOf<Project>.Random(),
            Name = "Project Three",
            Activities = new List<ActivityViewModel>()
        },
        new ProjectViewModel
        {
            Id = IdOf<Project>.Random(),
            Name = "Archived project",
            IsArchived = true,
            Activities = new List<ActivityViewModel>
            {
                new ActivityViewModel
                {
                    Id = IdOf<Activity>.Random(),
                    Name = "P1A",
                    Color = "#111111",
                    DarkerColor = "#222222",
                }
            }
        },
        new ProjectViewModel
        {
            Id = IdOf<Project>.Random(),
            Name = "Project with only archived activities",
            Activities = new List<ActivityViewModel>
            {
                new ActivityViewModel
                {
                    Id = IdOf<Activity>.Random(),
                    Name = "P1A",
                    Color = "#111111",
                    DarkerColor = "#222222",
                    IsArchived = true,
                }
            }
        },
    };
}