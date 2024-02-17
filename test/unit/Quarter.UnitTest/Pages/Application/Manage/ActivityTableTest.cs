using System.Linq;
using Bunit;
using NUnit.Framework;
using Quarter.Components;
using Quarter.Core.Models;
using Quarter.Pages.Application.Manage;
using Quarter.State;
using Quarter.State.ViewModels;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Application.Manage;

[TestFixture]
public class ActivityTableTest
{
    public class WhenRenderedEmpty : TestCase
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
        public void ItShouldShowEmptyCollectionMessage()
        {
            var emptyComponent = Component?.FindComponent<EmptyCollectionMessage>().Instance;

            Assert.Multiple(() =>
            {
                Assert.That(emptyComponent?.Header, Is.EqualTo("No activities"));
                Assert.That(emptyComponent?.Message, Is.EqualTo("You have not created any activities for this project yet."));
            });
        }
    }

    public class WhenRenderWithActivities : TestCase
    {
        private ProjectViewModel _projectViewModel;

        [OneTimeSetUp]
        public void Setup()
        {
            _projectViewModel = new ProjectViewModel
            {
                Id = IdOf<Project>.Random(),
                Name = "Project X",
                Activities = new []
                {
                    new ActivityViewModel { Name = "Activity One", TotalMinutes = 120, Color = "#123456", DarkerColor = "#000000" },
                    new ActivityViewModel { Name = "Activity Two", TotalMinutes = 0, Color = "#FFFFFF", DarkerColor = "#000000" },
                }
            };
            RenderWithParameters(pb => pb.Add(
                ps => ps.Project, _projectViewModel));
        }

        [Test]
        public void ItShouldListActivitiesForProject()
        {
            var activities = Component?.FindComponents<ActivityTableRow>()
                .Select(tr => tr.Instance.Activity?.Name);

            Assert.That(activities, Is.EqualTo(new [] { "Activity One", "Activity Two" }));
        }
    }

    public class WhenSelectingNewActivity : TestCase
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
            ClickAddActivityButton();
        }

        [Test]
        public void ItShouldDispatchAction()
            => Assert.That(DidDispatchAction(new ShowAddActivityAction(_projectViewModel.Id)), Is.True);
    }

    public class TestCase : BlazorComponentTestCase<ActivityTable>
    {
        protected void ClickAddActivityButton()
            => ComponentByTestAttribute("action-button")?.Click();
    }
}
