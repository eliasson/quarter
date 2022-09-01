using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using Bunit;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.Pages.Application.Timesheet;
using Quarter.State;
using Quarter.State.ViewModels;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Application.TimesheetPage;

[TestFixture]
public class TimesheetGridTest
{
    public class WhenEmpty : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            StateManager.State.SelectedTimesheet = Timesheet.CreateForDate(Date.Random());
            Render();
        }

        [Test]
        public void ItShouldHaveAStartOfDayActionEnabled()
            => Assert.That(StartOfDayAction()?.Attributes["disabled"], Is.Null);

        [Test]
        public void ItShouldHaveAEndOfDayActionEnable()
            => Assert.That(EndOfDayAction()?.Attributes["disabled"], Is.Null);

        [Test]
        public void ItShouldRenderDefaultHoursOnly()
        {
            var hours = RenderedHours();

            Assert.That(hours, Is.EqualTo(new []
            {
                "06:00", "07:00", "08:00", "09:00", "10:00", "11:00",
                "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00",
            }));
        }

        [Test]
        public void ItShouldHaveNoPendingCells()
            => Assert.That(PendingCells(), Is.Empty);
    }

    public class WhenClickingStartOfDay : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            StateManager.State.SelectedTimesheet = Timesheet.CreateForDate(Date.Random());
            Render();
            StartOfDayAction()?.Click();
        }

        [Test]
        public void ItShouldStartOneHourEarlierThanDefault()
            => Assert.That(RenderedHours()?.First(), Is.EqualTo("05:00"));
    }

    public class WhenClickingStartOfDayManyTimes : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            StateManager.State.SelectedTimesheet = Timesheet.CreateForDate(Date.Random());
            Render();
            foreach (var _ in Enumerable.Range(0, 24))
                StartOfDayAction()?.Click();
        }

        [Test]
        public void ItShouldStartAtMidnight()
            => Assert.That(RenderedHours()?.First(), Is.EqualTo("00:00"));

        [Test]
        public void ItShouldDisableStartOfDayAction()
            => Assert.That(StartOfDayAction()?.Attributes["disabled"], Is.Not.Null);
    }

    public class WhenClickingEndOfDay : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            StateManager.State.SelectedTimesheet = Timesheet.CreateForDate(Date.Random());
            Render();
            EndOfDayAction()?.Click();
        }

        [Test]
        public void ItShouldEndOneHourLaterThanDefault()
            => Assert.That(RenderedHours()?.Last(), Is.EqualTo("19:00"));
    }

    public class WhenClickingEndOfDayManyTimes : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            StateManager.State.SelectedTimesheet = Timesheet.CreateForDate(Date.Random());
            Render();
            foreach (var _ in Enumerable.Range(0, 24))
                EndOfDayAction()?.Click();
        }

        [Test]
        public void ItShouldEndOneHourToMidnight()
            => Assert.That(RenderedHours()?.Last(), Is.EqualTo("23:00"));

        [Test]
        public void ItShouldDisableEndOfDayAction()
            => Assert.That(EndOfDayAction()?.Attributes["disabled"], Is.Not.Null);
    }

    public class WhenRenderingTimesheet : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {

            var timesheet = Timesheet.CreateForDate(Date.Random());
            timesheet.Register(new ActivityTimeSlot(ListOfProjects[0].Id!, ListOfProjects[0].Activities[0].Id!, 40, 3));
            StateManager.State.SelectedTimesheet = timesheet;
            StateManager.State.Projects = ListOfProjects;
            Render();
        }

        [Test]
        public void ItShouldHaveNoPendingCells()
            => Assert.That(SelectedRange(), Is.Empty);

        [Test]
        public void ItShouldUseTheActivityColor()
        {
            var colors = Cells()?
                .Where(elm => elm.GetStyle()["border-color"].Length + elm.GetStyle()["background-color"].Length > 0)
                .Select(elm =>
                    (elm.GetStyle()["background-color"], elm.GetStyle()["border-color"]));

            Assert.That(colors, Is.EqualTo(new []
            {
                ("rgba(17, 17, 17, 1)", "rgba(34, 34, 34, 1)"),
                ("rgba(17, 17, 17, 1)", "rgba(34, 34, 34, 1)"),
                ("rgba(17, 17, 17, 1)", "rgba(34, 34, 34, 1)"),
            }));
        }
    }

    public class WhenRenderingTimesheetWithTimeRegisteredBeforeDefaultHours : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            var timesheet = Timesheet.CreateForDate(Date.Random());
            timesheet.Register(new ActivityTimeSlot(ListOfProjects[0].Id!, ListOfProjects[0].Activities[0].Id!, 0, 1));
            StateManager.State.SelectedTimesheet = timesheet;
            StateManager.State.Projects = ListOfProjects;
            Render();
        }

        [Test]
        public void ItShouldAdjustStartOfDayToFitRegisteredSlots()
            => Assert.That(RenderedHours()?.First(), Is.EqualTo("00:00"));
    }

    public class WhenRenderingTimesheetWithTimeRegisteredAfterDefaultHours : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            var timesheet = Timesheet.CreateForDate(Date.Random());
            timesheet.Register(new ActivityTimeSlot(ListOfProjects[0].Id!, ListOfProjects[0].Activities[0].Id!, 95, 1));
            StateManager.State.SelectedTimesheet = timesheet;
            StateManager.State.Projects = ListOfProjects;
            Render();
        }

        [Test]
        public void ItShouldAdjustStartOfDayToFitRegisteredSlots()
            => Assert.That(RenderedHours()?.Last(), Is.EqualTo("23:00"));
    }

    public class WhenReleasingQuarterSelectionWithNoSelectedActivity : TestCase
    {
        private readonly Date _dateInTest = Date.Random();

        [OneTimeSetUp]
        public async Task Setup()
        {
            StateManager.State.SelectedTimesheet = Timesheet.CreateForDate(_dateInTest);
            StateManager.State.Projects = ListOfProjects;
            Render();

            MouseDown(10);
            MouseOver(9);
            MouseOver(8);
            await MouseUp(8);
        }

        [Test]
        public void ItShouldHaveNoPendingCells()
            => Assert.That(SelectedRange(), Is.Empty);

        [Test]
        public void ItShouldDispatchEraseAction()
        {
            Assert.True(DidDispatchAction(new EraseTimeAction(_dateInTest, new EraseTimeSlot(8, 3))));
        }
    }

    public class WhenReleasingQuarterSelectionWithSelectedActivity : TestCase
    {
        private readonly Date _dateInTest = Date.Random();
        private readonly IdOf<Project> _projectId = ListOfProjects[0].Id;
        private readonly IdOf<Activity> _activityId = ListOfProjects[0].Activities[0].Id;

        [OneTimeSetUp]
        public async Task Setup()
        {
            StateManager.State.SelectedTimesheet = Timesheet.CreateForDate(_dateInTest);
            StateManager.State.SelectedActivity = new SelectedActivity(_projectId, _activityId);
            StateManager.State.Projects = ListOfProjects;
            Render();

            MouseDown(10);
            MouseOver(9);
            MouseOver(8);
            await MouseUp(8);
        }

        [Test]
        public void ItShouldHaveNoPendingCells()
            => Assert.That(SelectedRange(), Is.Empty);

        [Test]
        public void ItShouldDispatchEraseAction()
            => Assert.True(DidDispatchAction(new RegisterTimeAction(_dateInTest, new ActivityTimeSlot(_projectId, _activityId, 8, 3))));
    }

    public class TestCase : BlazorComponentTestCase<TimesheetGrid>
    {
        protected IElement StartOfDayAction()
            => Component?.Find("[test=start-of-day-action]");

        protected IElement EndOfDayAction()
            => Component?.Find("[test=end-of-day-action]");

        private IEnumerable<IElement> HourRows()
            => Component?.FindAll("[test=hour-row]");

        protected IEnumerable<string> RenderedHours()
            => HourRows()?.Select(row => row.QuerySelector(".qa-timesheet__time")?.TextContent);

        protected IRefreshableElementCollection<IElement> Cells()
            => Component?.FindAll("[test=timesheet-cell-activity]");

        protected IRefreshableElementCollection<IElement> PendingCells()
            => Component?.FindAll("[test=timesheet-cell-activity].qa-is-pending");

        protected int[] SelectedRange()
            => Component?.Instance.SelectedQuarterIdRange;

        // I could not get bunit to work with mouse events, so these events fake calls to the event handlers

        protected void MouseDown(int quarterIndex)
        {
            var quarter = GetQuarterAtIndex(quarterIndex);
            Component?.Instance.OnMouseDown(quarter!);
        }

        protected void MouseOver(int quarterIndex)
        {
            var quarter = GetQuarterAtIndex(quarterIndex);
            Component?.Instance.OnMouseOver(quarter!);
        }

        protected async Task MouseUp(int quarterIndex)
        {
            var quarter = GetQuarterAtIndex(quarterIndex);
            await Component!.Instance.OnMouseUp(quarter!);
        }

        private QuarterViewModel GetQuarterAtIndex(int quarterIndex)
        {
            var h = quarterIndex / 4;
            var q = quarterIndex % 4;
            return Component?.Instance.Hours().ToArray()[h].Quarters[q];
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
        }
    };
}