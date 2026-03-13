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
    public class AssignUserRoleCommandTest : CommandTestBase
    {
        public class WhenUserDoesNotExist : AssignUserRoleCommandTest
        {
            [Test]
            public void ShouldFail()
            {
                var command = new AssignUserRoleCommand(IdOf<User>.Random(), UserRole.Administrator);
                Assert.ThrowsAsync<NotFoundException>(() => Handler.ExecuteAsync(command, OperationContext(), CancellationToken.None));
            }
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

        }
    }
}
