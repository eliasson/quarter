using System.Threading;
using System.Threading.Tasks;
using Quarter.Core.Commands;
using Quarter.Core.Exceptions;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using NUnit.Framework;

namespace Quarter.Core.UnitTest.Commands
{
    [TestFixture]
    public class RevokeRoleCommandTest : CommandTestBase
    {
        public class WhenUserDoesNotExist : RevokeRoleCommandTest
        {
            [Test]
            public void ItShouldFail()
            {
                var command = new AssignUserRoleCommand(IdOf<User>.Random(), UserRole.Administrator);
                Assert.ThrowsAsync<NotFoundException>(() => Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None));
            }
        }

        public class WhenUserExist : RevokeRoleCommandTest
        {
            private User _initialUser;

            [OneTimeSetUp]
            public async Task AddingInitialUser()
            {
                _initialUser = await AddUserInRepository(new Email("jane.doe@example.com"), UserRole.Administrator);
                var command = new RevokeUserRoleCommand(_initialUser.Id, UserRole.Administrator);
                await Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None);
            }

            [Test]
            public async Task ItShouldHaveUpdatedUserWithoutRole()
            {
                var user = await UserRepository.GetByIdAsync(_initialUser.Id, CancellationToken.None);
                Assert.That(user.Roles, Is.Empty);
            }
        }
    }
}