using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using Bunit;
using NUnit.Framework;
using Quarter.Components;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.Pages.Application.Timesheet;
using Quarter.State.ViewModels;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Application.TimesheetPage;

public abstract class TimesheetSummaryWidgetTest
{
    public class WhenNoTimesheetIsRegistered : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
            => Render();

        [Test]
        public void ItShouldHaveTotalSummaryHoursOfZero()
            => Assert.That(TotalSummary()?.TextContent, Is.EqualTo("0.00"));

        [Test]
        public void ItShouldHaveAnEmptyMessage()
        {
            var emptyComponent = Component?.FindComponent<EmptyCollectionMessage>().Instance;

            Assert.That(emptyComponent?.Message, Is.EqualTo("No time registered."));
        }
    }

    public class WhenTimesheetIsRegistered : TestCase
    {
        private Project _project1;
        private Project _project2;
        private Activity _activityP1A;
        private Activity _activityP2A;
        private Activity _activityP2B;

        [OneTimeSetUp]
        public void Setup()
        {
            _project1 = new Project("Project One", "Irrelevant");
            _activityP1A = new Activity(_project1.Id, "P1A", "", Color.FromHexString("#123123"));
            _project2 = new Project("Project Two", "Irrelevant");
            _project2.Archive();
            _activityP2A = new Activity(_project2.Id, "P2A", "", Color.FromHexString("#aaa"));
            _activityP2A.Archive();
            _activityP2B = new Activity(_project2.Id, "P2B", "", Color.FromHexString("#bbb"));

            StateManager.State.Projects = TestProjects();
            StateManager.State.SelectedTimesheet = Timesheet.CreateForDate(Date.Today());
            StateManager.State.SelectedTimesheet.Register(new ActivityTimeSlot(_project1.Id, _activityP1A.Id, 0, 2)); // 0.5
            StateManager.State.SelectedTimesheet.Register(new ActivityTimeSlot(_project2.Id, _activityP2A.Id, 2, 2)); // 0.5
            StateManager.State.SelectedTimesheet.Register(new ActivityTimeSlot(_project2.Id, _activityP2B.Id, 4, 42)); // 10.5

            Render();
        }

        [Test]
        public void ItShouldHaveTotalSummaryHours()
            => Assert.That(TotalSummary()?.TextContent, Is.EqualTo("11.50"));

        [Test]
        public void ItShouldListProjectTotals()
            => Assert.That(ProjectTotals(), Is.EqualTo(new[]
            {
                ("Project One", "0.50"),
                ("Project Two", "11.00"),
            }));

        [Test]
        public void ItShouldHaveArchivedIconsForProjects()
        {
            Assert.That(ArchivedProjectMarkers(), Is.EqualTo(new[]
            {
                ("Project One", false),
                ("Project Two", true),
            }));
        }
        [Test]
        public void ItShouldListTotalPerActivity()
            => Assert.That(ActivityTotals(), Is.EqualTo(new[]
            {
                ("P1A", "0.50"),
                ("P2A", "0.50"),
                ("P2B", "10.50"),
            }));

        [Test]
        public void ItShouldHaveActivityMarkers()
        {
            Assert.That(ActivityMarkers(), Is.EqualTo(new[]
            {
                ("P1A", "rgba(17, 17, 17, 1)", "rgba(34, 34, 34, 1)"),
                ("P2A", "rgba(51, 51, 51, 1)", "rgba(68, 68, 68, 1)"),
                ("P2B", "rgba(85, 85, 85, 1)", "rgba(102, 102, 102, 1)"),
            }));
        }

        [Test]
        public void ItShouldHaveArchivedIconsForActivities()
        {
            Assert.That(ArchivedActivityMarkers(), Is.EqualTo(new[]
            {
                ("P1A", false),
                ("P2A", true),
                ("P2B", false),
            }));
        }

        private List<ProjectViewModel> TestProjects()
        {
            return new List<ProjectViewModel>
            {
                new ProjectViewModel
                {
                    Id = _project1.Id,
                    Name = _project1.Name,
                    IsArchived = _project1.IsArchived,
                    Activities = new List<ActivityViewModel>
                    {
                        new ActivityViewModel
                        {
                            Id = _activityP1A.Id,
                            Name = _activityP1A.Name,
                            Color = "#111111",
                            DarkerColor = "#222222",
                            IsArchived = _activityP1A.IsArchived
                        }
                    }
                },
                new ProjectViewModel
                {
                    Id = _project2.Id,
                    Name = _project2.Name,
                    IsArchived = _project2.IsArchived,
                    Activities = new List<ActivityViewModel>
                    {
                        new ActivityViewModel
                        {
                            Id = _activityP2A.Id,
                            Name = _activityP2A.Name,
                            Color = "#333333",
                            DarkerColor = "#444444",
                            IsArchived = _activityP2A.IsArchived
                        },
                        new ActivityViewModel
                        {
                            Id = _activityP2B.Id,
                            Name = _activityP2B.Name,
                            Color = "#555555",
                            DarkerColor = "#666666",
                            IsArchived = _activityP2B.IsArchived
                        }
                    }
                }
            };
        }
    }

    public class TestCase : BlazorComponentTestCase<TimesheetSummaryWidget>
    {
        protected IElement TotalSummary()
            => Component?.Find("[test=summary-total]");

        protected (string Name, string Total)[] ProjectTotals()
        {
            var result = Component?.FindAll("[test=summary-project]").Select(elm =>
            {
                var name = elm.QuerySelector("[test=summary-name]")?.TextContent;
                var total = elm.QuerySelector("[test=summary-total]")?.TextContent;
                return (name, total);
            }).ToArray();

            return result ?? Array.Empty<(string, string)>();
        }

        protected (string Name, bool IsAcchived)[] ArchivedProjectMarkers()
        {
            var result = Component?.FindAll("[test=summary-project]").Select(elm =>
            {
                var name = elm.QuerySelector("[test=summary-name]")?.TextContent;
                var archived = elm.QuerySelector("[test=summary-archived]") is not null;
                return (name, archived);
            }).ToArray();

            return result ?? Array.Empty<(string, bool)>();
        }

        protected (string Name, string Total)[] ActivityTotals()
        {
            var result = Component?.FindAll("[test=summary-activity]").Select(elm =>
            {
                var name = elm.QuerySelector("[test=summary-name]")?.TextContent;
                var total = elm.QuerySelector("[test=summary-total]")?.TextContent;
                return (name, total);
            }).ToArray();

            return result ?? Array.Empty<(string, string)>();
        }

        protected IEnumerable<(string title, string bgColor, string borderColor)> ActivityMarkers()
        {
            return Component?.FindAll("[test=summary-activity]").Select(item =>
            {
                var title = item.QuerySelector("[test=summary-name]")?.TextContent;
                var bgColor = item.QuerySelector("[test=activity-item-marker]").GetStyle()["background-color"];
                var borderColor = item.QuerySelector("[test=activity-item-marker]").GetStyle()["border-color"];
                return (title, bgColor, borderColor);
            });
        }

        protected (string Name, bool IsAcchived)[] ArchivedActivityMarkers()
        {
            var result = Component?.FindAll("[test=summary-activity]").Select(elm =>
            {
                var name = elm.QuerySelector("[test=summary-name]")?.TextContent;
                var archived = elm.QuerySelector("[test=summary-archived]") is not null;
                return (name, archived);
            }).ToArray();

            return result ?? Array.Empty<(string, bool)>();
        }
    }
}