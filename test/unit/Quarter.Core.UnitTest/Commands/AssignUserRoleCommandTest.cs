using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Commands;
using Quarter.Core.Events;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using NUnit.Framework;

namespace Quarter.Core.UnitTest.Commands
{
    public abstract class AssignUserRoleCommandTest : CommandTestBase<AssignedUserRoleEvent>
    {
        public class WhenUserDoesNotExist : AssignUserRoleCommandTest
        {
            [Test]
            public void ShouldFail()
            {
                var command = new AssignUserRoleCommand(IdOf<User>.Random(), UserRole.Administrator);
                Assert.ThrowsAsync<NotFoundException>(() => Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None));
            }

            [Test]
            public void ItShouldNotDispatchAnyEvent()
                => Assert.That(EventSubscriber.CollectedEvents, Is.Empty);
        }

        public class WhenUserExist : AssignUserRoleCommandTest
        {
            private User _initialUser;

            [OneTimeSetUp]
            public async Task AddingInitialUser()
            {
                _initialUser = await AddUserInRepository(new Email("jane.doe@example.com"));
                var command = new AssignUserRoleCommand(_initialUser.Id, UserRole.Administrator);

                await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
            }

            [Test]
            public async Task ItShouldHaveUpdatedUserWithRole()
            {
                var user = await UserRepository.GetByIdAsync(_initialUser.Id, CancellationToken.None);
                Assert.That(user.Roles, Does.Contain(UserRole.Administrator));
            }

            [Test]
            public async Task ItShouldDispatchUserRemovedEvent()
            {
                var (userId, userRoles) = await EventSubscriber.EventuallyDispatchedOneEvent();

                Assert.Multiple(() =>
                {
                    Assert.That(userId, Is.EqualTo(_initialUser.Id));
                    Assert.That(userRoles, Is.EqualTo(new [] { UserRole.Administrator }));
                });
            }
        }
    }
}