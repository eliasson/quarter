using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.State;
using Quarter.State.ViewModels;

namespace Quarter.UnitTest.State;

[TestFixture]
public class LoadProjectsActionTest
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
            => Assert.That(_projectViewModel.LastUsed, Is.EqualTo(UtcDateTime.MinValue));

        [Test]
        public void ItShouldIncludeTotalMinutes()
            => Assert.That(_projectViewModel.TotalMinutes, Is.Zero);

        [Test]
        public void ItShouldHaveZeroActivities()
            => Assert.That(_projectViewModel.Activities, Is.Empty);

        [Test]
        public void ItShouldNotBeArchived()
            => Assert.That(_projectViewModel.IsArchived, Is.False);
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

    public class WithArchivedProject : TestCase
    {
        private Project _project;
        private ProjectViewModel _projectViewModel;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _project = await AddProject(ActingUserId, "Alpha", "Alpha description");
            _project.Archive();
            _project = await UpdateProject(ActingUserId, _project.Id);
            State = await ActionHandler.HandleAsync(State, new LoadProjects(), CancellationToken.None);
            _projectViewModel = State.Projects.First();
        }

        [Test]
        public void ItShouldNotBeArchived()
            => Assert.That(_projectViewModel.IsArchived, Is.True);
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
        public void ItShouldNotBeArchived()
            => Assert.That(_activityViewModel.IsArchived, Is.False);
    }

    public class WithUsedActivity : TestCase
    {
        private Activity _activity;
        private ActivityViewModel _activityViewModel;
        private ProjectViewModel _projectViewModel;
        private Date _dateInTest;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _dateInTest = Date.Today();
            var project = await AddProject(ActingUserId, "Alpha", "Alpha description");
            _activity = await AddActivity(ActingUserId, project.Id, "Activity One", "Some activity",
                Color.FromHexString("#333"));
            _activity = await UpdateActivity(ActingUserId, _activity.Id);
            await AddTimesheet(ActingUserId, _dateInTest, project.Id, _activity.Id, 0, 6);

            _activity.Archive();
            _activity = await UpdateActivity(ActingUserId, _activity.Id);

            State = await ActionHandler.HandleAsync(State, new LoadProjects(), CancellationToken.None);

            _projectViewModel = State.Projects.First();
            _activityViewModel = _projectViewModel.Activities.First();
        }

        [Test]
        public void ItShouldUseUpdatedTimestampAsUpdated()
            => Assert.That(_activityViewModel.Updated, Is.EqualTo(_activity.Updated));

        [Test]
        public void ItShouldIncludeActivityTotalMinutes()
            => Assert.That(_activityViewModel.TotalMinutes, Is.EqualTo(90));


        [Test]
        public void ItShouldNotHaveLastUsedTimestamp()
            => Assert.That(_projectViewModel.LastUsed!.Value.DateTime - DateTime.UtcNow, Is.LessThan(TimeSpan.FromMilliseconds(500)));

        [Test]
        public void ItShouldIncludeTotalMinutes()
            => Assert.That(_projectViewModel.TotalMinutes, Is.EqualTo(90));

        [Test]
        public void ItShouldBeArchived()
            => Assert.That(_activityViewModel.IsArchived, Is.True);
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
