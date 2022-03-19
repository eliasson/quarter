using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Commands;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using NUnit.Framework;

namespace Quarter.Core.UnitTest.Commands
{
    public abstract class RemoveUserCommandTest : CommandTestBase
    {
        public class WhenUserDoesNotExist : RemoveUserCommandTest
        {
            [Test]
            public void ItShouldNotFail()
            {
                var command = new RemoveUserCommand(IdOf<User>.Random());
                Assert.DoesNotThrowAsync(() => Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None));
            }
        }

        public class WhenUserExist : RemoveUserCommandTest
        {
            private User _initialUser;

            [OneTimeSetUp]
            public async Task AddingInitialUser()
            {
                _initialUser = await AddUserInRepository(new Email("jane.doe@example.com"));
                var command = new RemoveUserCommand(_initialUser.Id);

                await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
            }

            [Test]
            public void ItShouldHaveRemovedUser()
                => Assert.ThrowsAsync<NotFoundException>(() => UserRepository.GetByIdAsync(_initialUser.Id, CancellationToken.None));
        }
    }
}