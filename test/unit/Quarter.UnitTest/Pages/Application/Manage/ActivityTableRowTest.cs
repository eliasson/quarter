using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using NUnit.Framework;
using Quarter.Components;
using Quarter.Pages.Application.Manage;
using Quarter.State;
using Quarter.State.ViewModels;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Application.Manage;

public abstract class ActivityTableRowTest
{
    public class WhenRendered : TestCase
    {
        private ActivityViewModel _activityViewModel;

        [OneTimeSetUp]
        public void Setup()
        {
            _activityViewModel = new ActivityViewModel
            {
                Name = "Activity One",
                TotalMinutes = 75,
                Color = "#123456",
                DarkerColor = "#000000",
            };
            RenderWithParameters(bp =>
                bp.Add(ps => ps.Activity, _activityViewModel));
        }

        [Test]
        public void ItShouldRenderActivityColor()
            => Assert.That(ActivityColorStyle(), Is.EqualTo("background-color: #123456; border-color: #000000"));

        [Test]
        public void ItShouldRenderActivityName()
            => Assert.That(ActivityName(), Is.EqualTo("Activity One"));

        [Test]
        public void ItShouldRenderActivityTotalUsage()
            => Assert.That(ActivityTotalUsage(), Is.EqualTo("1.25 h"));

        [Test]
        public void ItShouldHaveActivityMenuItems()
        {
            Assert.That(MenuItems(), Is.EqualTo(new []
            {
                ("edit", "Edit activity"),
                ("remove", "Remove activity"),
            }));
        }

        public class WhenSelectingEditMenuItem : WhenRendered
        {
            [OneTimeSetUp]
            public void SelectItem()
                => MenuItem("Edit activity").Click();

            [Test]
            public async Task ItShouldDispatchAction()
                => Assert.True(await EventuallyDispatchedAction(new ShowEditActivityAction(_activityViewModel.ProjectId, _activityViewModel.Id)));
        }

        public class WhenSelectingRemoveMenuItem : WhenRendered
        {
            [OneTimeSetUp]
            public void SelectItem()
                => MenuItem("Remove activity").Click();

            [Test]
            public async Task ItShouldDispatchAction()
                => Assert.True(await EventuallyDispatchedAction(new ShowRemoveActivityAction(_activityViewModel.Id)));
        }
    }

    public class TestCase : BlazorComponentTestCase<ActivityTableRow>
    {
        private IRenderedComponent<ContextMenu> ContextMenu()
            => Component?.FindComponent<ContextMenu>();

        protected IEnumerable<(string, string)> MenuItems()
        {
            var menu = ContextMenu();
            return  menu?.Instance.Items.Select(item => (item.Type, item.Title));
        }

        protected string ActivityColorStyle()
            => ComponentByTestAttribute("activity-color").Attributes["style"]?.Value;

        protected string ActivityName()
            => ComponentByTestAttribute("activity-name").TextContent;

        protected string ActivityTotalUsage()
            => ComponentByTestAttribute("activity-usage").TextContent;

        protected IElement MenuItem(string title)
        {
            ComponentByTestAttribute("menu-launcher").Click();
            return Component?.FindAll("li").First(m => m.Attributes["test-menu"]?.Value == title);
        }
    }
}