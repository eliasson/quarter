using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Commands;
using Quarter.Core.Events;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using NUnit.Framework;

namespace Quarter.Core.UnitTest.Commands
{
    public abstract class AddUserCommandTest : CommandTestBase<UserCreatedEvent>
    {
        public class WhenUserDoesNotExist : AddUserCommandTest
        {
            [OneTimeSetUp]
            public async Task AddingUser()
            {
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