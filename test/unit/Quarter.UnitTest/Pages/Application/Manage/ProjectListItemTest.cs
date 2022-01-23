using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using Bunit.Rendering;
using NUnit.Framework;
using Quarter.Components;
using Quarter.Core.Models;
using Quarter.Pages.Application.Manage;
using Quarter.State;
using Quarter.State.ViewModels;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Application.Manage;

public abstract class ProjectListItemTest
{
    public class WhenRenderMinimalProject : TestCase
    {
        private ProjectViewModel _projectViewModel;

        [OneTimeSetUp]
        public void Setup()
        {
            _projectViewModel = new ProjectViewModel
            {
                Id = IdOf<Project>.Random(),
                Name = "Project X",
            };
            RenderWithParameters(pb => pb.Add(
                ps => ps.Project, _projectViewModel));
        }

        [Test]
        public void ItShouldHaveTitle()
            => Assert.That(Title(), Is.EqualTo("Project X"));

        [TestCase(0, "Hours", "0.00")]
        [TestCase(1, "Activities", "0")]
        [TestCase(2, "Updated at", "-")]
        [TestCase(3, "Last used at", "-")]
        public void ItShouldRenderProjectStats(int index, string expectedUnit, string expectedValue)
        {
            var category = CategoryByIndex(index);
            var unit = category?.QuerySelector("[test=project-unit]")?.TextContent;
            var value = category?.QuerySelector("[test=project-value]")?.TextContent;

            Assert.Multiple(() =>
            {
                Assert.That(unit, Is.EqualTo(expectedUnit));
                Assert.That(value, Is.EqualTo(expectedValue));
            });
        }

        public class Initially : WhenRenderMinimalProject
        {
            [Test]
            public void ItShouldNotBeActive()
                => Assert.That(IsActive(), Is.False);

            [Test]
            public void ItShouldHaveProjectMenuItems()
            {
                Assert.That(MenuItems(), Is.EqualTo(new []
                {
                    ("edit", "Edit project"),
                    ("remove", "Remove project"),
                }));
            }

            [Test]
            public void ItShouldNotShowAnActivityTable()
                => Assert.Catch<ComponentNotFoundException>(() => Component?.FindComponent<ActivityTable>());
        }

        public class WhenSelectingRemoveProjectMenuItem : WhenRenderMinimalProject
        {
            [OneTimeSetUp]
            public void SelectItem()
                => MenuItem("Remove project").Click();

            [TestCase]
            public async Task ItShouldDispatchShowRemoveProjectAction()
                => Assert.True(await EventuallyDispatchedAction(new ShowRemoveProjectAction(_projectViewModel.Id)));
        }

        public class WhenSelectingEditProjectMenuItem : WhenRenderMinimalProject
        {
            [OneTimeSetUp]
            public void SelectItem()
                => MenuItem("Edit project").Click();

            [Test]
            public async Task ItShouldDispatchShowEditProjectAction()
                => Assert.True(await EventuallyDispatchedAction(new ShowEditProjectAction(_projectViewModel.Id)));
        }

        public class WhenSelectingProject : WhenRenderMinimalProject
        {
            [OneTimeSetUp]
            public void SetupSelectedProject()
                => SelectProjectItem();

            [Test]
            public void ItShouldBeActive()
                => Assert.That(IsActive(), Is.True);

            [Test]
            public void ItShouldShowAnActivityTable()
                => Assert.DoesNotThrow(() => Component?.FindComponent<ActivityTable>());
        }
    }

    public abstract class TestCase : BlazorComponentTestCase<ProjectListItem>
    {
        protected string Title()
            => ComponentByTestAttribute("project-title")?.TextContent;

        protected IElement CategoryByIndex(int index)
            => ComponentsByTestAttribute("project-category")?[index];

        protected void SelectProjectItem()
            => ComponentByTestAttribute("project-title")?.Click();

        private IRenderedComponent<ContextMenu> ProjectContextMenu()
            => Component?.FindComponent<ContextMenu>();

        protected bool IsActive()
        {
            if (Component is not null)
            {
                var classes = ComponentByTestAttribute("project-list-item")?.ClassList;
                return classes?.Contains("qa-list-item--is-active") ?? false;
            }
            return false;
        }

        protected IEnumerable<(string, string)> MenuItems()
        {
            var menu = ProjectContextMenu();
            return  menu?.Instance.Items.Select(item => (item.Type, item.Title));
        }

        protected IElement MenuItem(string title)
        {
            ComponentByTestAttribute("menu-launcher").Click();
            return Component?.FindAll("li").First(m => m.Attributes["test-menu"]?.Value == title);
        }
    }
}