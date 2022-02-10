using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Commands;
using Quarter.Core.Events;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using NUnit.Framework;
using Quarter.Core.UnitTest.TestUtils;

namespace Quarter.Core.UnitTest.Commands
{
    public abstract class AddUserCommandTest : CommandTestBase<UserCreatedEvent>
    {
        public class WhenUserDoesNotExist : AddUserCommandTest
        {
            private TestSubscriber<ProjectCreatedEvent> _projectEventSubscriber;
            private TestSubscriber<ActivityCreatedEvent> _activityEventSubscriber;

            [OneTimeSetUp]
            public async Task AddingUser()
            {
                _projectEventSubscriber = new TestSubscriber<ProjectCreatedEvent>(EventDispatcher);
                _activityEventSubscriber = new TestSubscriber<ActivityCreatedEvent>(EventDispatcher);
                var command = new AddUserCommand(new Email("jane.doe@example.com"), User.NoRoles);
                await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
            }

            [Test]
            public async Task ItShouldHaveAddedTheUser()
            {
                var user = await UserRepository.GetUserByEmailAsync("jane.doe@example.com", CancellationToken.None);

                Assert.That(user, Is.Not.Null);
            }

            [Test]
            public void ItShouldHaveDispatchedUserCreatedEvent()
            {
                var ev = EventSubscriber.CollectedEvents.Single();

                Assert.That(ev.User.Email.Value, Is.EqualTo("jane.doe@example.com"));
            }

            [Test]
            public async Task ItShouldHaveCreatedProjectForUser()
            {
                var user = await UserRepository.GetUserByEmailAsync("jane.doe@example.com", CancellationToken.None);
                var projectRepositoryForUser = RepositoryFactory.ProjectRepository(user.Id);
                var projects = await projectRepositoryForUser.GetAllAsync(CancellationToken.None).ToListAsync(CancellationToken.None);
                var names = projects.Select(p => p.Name);

                Assert.That(names, Is.EqualTo(new [] { "Your first project" }));
            }

            [Test]
            public void ItShouldHaveDispatchedProjectCreatedEvent()
            {
                var ev = _projectEventSubscriber.CollectedEvents.Single();

                Assert.That(ev.Project.Name, Is.EqualTo("Your first project"));
            }

            [Test]
            public async Task ItShouldHaveCreatedActivityForUser()
            {
                var user = await UserRepository.GetUserByEmailAsync("jane.doe@example.com", CancellationToken.None);
                var projectRepositoryForUser = RepositoryFactory.ActivityRepository(user.Id);
                var activities = await projectRepositoryForUser.GetAllAsync(CancellationToken.None).ToListAsync(CancellationToken.None);
                var names = activities.Select(p => p.Name);

                Assert.That(names, Is.EqualTo(new [] { "Your first activity" }));
            }

            [Test]
            public void ItShouldHaveDispatchedActivityCreatedEvent()
            {
                var ev = _activityEventSubscriber.CollectedEvents.Single();

                Assert.That(ev.Activity.Name, Is.EqualTo("Your first activity"));
            }
        }

        public class WhenUserExist : AddUserCommandTest
        {
            [OneTimeSetUp]
            public Task AddingInitialUser()
                => AddUserInRepository(new Email("jane.doe@example.com"));

            [Test]
            public void ItShouldFailWhenAddingSameUser()
            {
                var command = new AddUserCommand(new Email("jane.doe@example.com"), User.NoRoles);
                Assert.CatchAsync<ArgumentException>(() => Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None));
            }

            [Test]
            public void ItShouldNotDispatchAnyEvent()
                => Assert.That(EventSubscriber.CollectedEvents, Is.Empty);
        }
    }
}