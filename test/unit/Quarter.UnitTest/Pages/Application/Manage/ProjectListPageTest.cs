using System.Collections.Generic;
using System.Linq;
using Bunit;
using Bunit.Rendering;
using NUnit.Framework;
using Quarter.Components;
using Quarter.Pages.Application.Manage;
using Quarter.State;
using Quarter.State.ViewModels;
using Quarter.UnitTest.TestUtils;
using Quarter.Utils;

namespace Quarter.UnitTest.Pages.Application.Manage;

[TestFixture]
public class ProjectListPageTest
{
    [TestFixture]
    public class WhenRenderedEmpty : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Render();
        }

        [Test]
        public void ItHasAProjectTab()
        {
            var context = Component?.FindComponent<PageContext>();

            Assert.That(context?.Instance.Tabs, Is.EquivalentTo(new [] {
                new TabData ("Projects", Page.Manage)}
            ));
        }

        [Test]
        public void ItHasAnEmptyMessage()
        {
            var emptyComponent = Component?.FindComponent<EmptyCollectionMessage>().Instance;

            Assert.Multiple(() =>
            {
                Assert.That(emptyComponent?.Header, Is.EqualTo("No projects"));
                Assert.That(emptyComponent?.Message, Is.EqualTo("You have not created any projects yet."));
            });
        }
    }

    public class WhenRenderedWithProjects : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            StateManager.State.Projects = new List<ProjectViewModel>
            {
                new ProjectViewModel { Name = "Alpha" },
                new ProjectViewModel { Name = "Bravo" },
                new ProjectViewModel { Name = "Charlie" },
            };
            Render();
        }

        [Test]
        public void ItShouldListAllProjects()
            => Assert.That(ProjectNames(),Is.EqualTo(new [] { "Alpha", "Bravo", "Charlie" }));

        [Test]
        public void ItShouldNotShowAnEmptyMessage()
            => Assert.Catch<ComponentNotFoundException>(() => Component?.FindComponent<EmptyCollectionMessage>());
    }

    public class WhenClickingAddProjectButton : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Render();
            ClickActionButton();
        }

        [Test]
        public void ShouldDispatchAction()
            => Assert.That(DidDispatchAction(new ShowAddProjectAction()), Is.True);
    }

    public abstract class TestCase : BlazorComponentTestCase<ProjectListPage>
    {
        protected IEnumerable<string> ProjectNames()
        {
            var items = Component?.FindComponents<ProjectListItem>();
            return items?.Select(i => i.Instance.Project?.Name);
        }

        protected void ClickActionButton()
            => ComponentByTestAttribute("action-button")?.Click();
    }
}
