using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.State;
using Quarter.State.ViewModels;

namespace Quarter.UnitTest.State;

public abstract class WhenDispatchingLoadProjectsActionTest
{
    public class WithDefaultBehaviour : TestCase
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            await AddProject(ActingUserId, "Alpha One", "Alpha alpha");
            await AddProject(ActingUserId, "Bravo One", "Bravo bravo");
            await AddProject(IdOf<User>.Random(), "Stray", "Not visible");

            State = await ActionHandler.HandleAsync(State, new LoadProjects(), CancellationToken.None);

            await AddProject(ActingUserId, "Charlie One", "Charlie charlie");
            State = await ActionHandler.HandleAsync(State, new LoadProjects(), CancellationToken.None);
        }

        [Test]
        public void ItShouldIncludeProjectsInState()
        {
            var projectNames = State.Projects.Select(p => p.Name);
            Assert.That(projectNames, Is.EquivalentTo(new[]
            {
                "Alpha One",
                "Bravo One",
            }));
        }
    }

    public class WhenForceRefresh : TestCase
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            await AddProject(ActingUserId, "Alpha One", "Alpha alpha");
            await AddProject(ActingUserId, "Bravo One", "Bravo bravo");
            await AddProject(IdOf<User>.Random(), "Stray", "Not visible");

            State = await ActionHandler.HandleAsync(State, new LoadProjects(), CancellationToken.None);

            await AddProject(ActingUserId, "Charlie One", "Charlie charlie");
            State = await ActionHandler.HandleAsync(State, new LoadProjects(Force: true), CancellationToken.None);
        }

        [Test]
        public void ItShouldIncludeProjectsInState()
        {
            var projectNames = State.Projects.Select(p => p.Name);
            Assert.That(projectNames, Is.EquivalentTo(new[]
            {
                "Alpha One",
                "Bravo One",
                "Charlie One",
            }));
        }
    }

    public class WithMinimalProject : TestCase
    {
        private Project _project;
        private ProjectViewModel _projectViewModel;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _project = await AddProject(ActingUserId, "Alpha", "Alpha description");
            State = await ActionHandler.HandleAsync(State, new LoadProjects(), CancellationToken.None);
            _projectViewModel = State.Projects.First();
        }

        [Test]
        public void ItShouldIncludeProjectId()
            => Assert.That(_projectViewModel.Id, Is.EqualTo(_project.Id));

        [Test]
        public void ItShouldIncludeProjectName()
            => Assert.That(_projectViewModel.Name, Is.EqualTo("Alpha"));

        [Test]
        public void ItShouldIncludeProjectDescription()
            => Assert.That(_projectViewModel.Description, Is.EqualTo("Alpha description"));

        [Test]
        public void ItShouldUseCreatedTimestampAsUpdated()
            => Assert.That(_projectViewModel.Updated, Is.EqualTo(_project.Created));

        [Test]
        public void ItShouldNotHaveLastUsedTimestamp()
            => Assert.That(_projectViewModel.LastUsed, Is.Null);

        [Test]
        public void ItShouldIncludeTotalMinutes()
            => Assert.That(_projectViewModel.TotalMinutes, Is.Zero);

        [Test]
        public void ItShouldIncludeTotalHours()
            => Assert.That(_projectViewModel.TotalAsHours(), Is.EqualTo("0.00"));

        [Test]
        public void ItShouldHaveZeroActivities()
            => Assert.That(_projectViewModel.Activities, Is.Empty);
    }

    public class WithFullProject : TestCase
    {
        private Project _project;
        private ProjectViewModel _projectViewModel;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _project = await AddProject(ActingUserId, "Alpha", "Alpha description");
            _project = await UpdateProject(ActingUserId, _project.Id);
            State = await ActionHandler.HandleAsync(State, new LoadProjects(), CancellationToken.None);
            _projectViewModel = State.Projects.First();
        }

        [Test]
        public void ItShouldUseUpdatedTimestampAsUpdated()
            => Assert.That(_projectViewModel.Updated, Is.EqualTo(_project.Updated));
    }

    public class WithMinimalActivity : TestCase
    {
        private Activity _activity;
        private ActivityViewModel _activityViewModel;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var project = await AddProject(ActingUserId, "Alpha", "Alpha description");
            _activity = await AddActivity(ActingUserId, project.Id, "Activity One", "Some activity", Color.FromHexString("#333"));
            State = await ActionHandler.HandleAsync(State, new LoadProjects(), CancellationToken.None);

            var projectVm = State.Projects.First();
            _activityViewModel = projectVm.Activities.First();
        }

        [Test]
        public void ItShouldIncludeActivityId()
            => Assert.That(_activityViewModel.Id, Is.EqualTo(_activity.Id));

        [Test]
        public void ItShouldIncludeActivityProjectId()
            => Assert.That(_activityViewModel.ProjectId, Is.EqualTo(_activity.ProjectId));

        [Test]
        public void ItShouldIncludeActivityName()
            => Assert.That(_activityViewModel.Name, Is.EqualTo("Activity One"));

        [Test]
        public void ItShouldIncludeActivityDescription()
            => Assert.That(_activityViewModel.Description, Is.EqualTo("Some activity"));

        [Test]
        public void ItShouldContainActivityColor()
            => Assert.That(_activityViewModel.Color, Is.EqualTo("#333333"));

        [Test]
        public void ItShouldContainDarkerActivityColor()
            => Assert.That(_activityViewModel?.DarkerColor, Is.EqualTo("#2B2B2B"));

        [Test]
        public void ItShouldUseCreatedTimestampAsUpdated()
            => Assert.That(_activityViewModel.Updated, Is.EqualTo(_activity.Created));

        [Test]
        public void ItShouldNotHaveLastUsedTimestamp()
            => Assert.That(_activityViewModel.LastUsed, Is.Null);

        [Test]
        public void ItShouldIncludeTotalMinutes()
            => Assert.That(_activityViewModel.TotalMinutes, Is.Zero);

        [Test]
        public void ItShouldIncludeTotalHours()
            => Assert.That(_activityViewModel.TotalAsHours(), Is.EqualTo("0.00"));
    }

    public class WithUsedActivity : TestCase
    {
        private Activity _activity;
        private ActivityViewModel _activityViewModel;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var project = await AddProject(ActingUserId, "Alpha", "Alpha description");
            _activity = await AddActivity(ActingUserId, project.Id, "Activity One", "Some activity",
                Color.FromHexString("#333"));
            _activity = await UpdateActivity(ActingUserId, _activity.Id);
            State = await ActionHandler.HandleAsync(State, new LoadProjects(), CancellationToken.None);

            var projectVm = State.Projects.First();
            _activityViewModel = projectVm.Activities.First();
        }

        [Test]
        public void ItShouldUseUpdatedTimestampAsUpdated()
            => Assert.That(_activityViewModel.Updated, Is.EqualTo(_activity.Updated));
    }

    public class TestCase : ActionHandlerTestCase
    {
        protected ApplicationState State;

        [OneTimeSetUp]
        public void SetupTestCase()
        {
            State = NewState();
        }

        protected async Task<Project> UpdateProject(IdOf<User> userId, IdOf<Project> projectId)
        {
            var repo = RepositoryFactory.ProjectRepository(userId);
            return await repo.UpdateByIdAsync(projectId, p =>
            {
                p.Name += " (updated)";
                return p;
            }, CancellationToken.None);
        }

        protected async Task<Activity> UpdateActivity(IdOf<User> userId, IdOf<Activity> activityId)
        {
            var repo = RepositoryFactory.ActivityRepository(userId);
            return await repo.UpdateByIdAsync(activityId, a =>
            {
                a.Name += " (updated)";
                return a;
            }, CancellationToken.None);
        }
    }
}