using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class GetUsersTest
{
    [TestFixture]
    public class WhenUserIsAdmin : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();

        [OneTimeSetUp]
        public async Task Setup()
        {
            await AddUser("jane.doe@example.com", u => u.AssignRole(UserRole.Administrator));
            await AddUser("john.doe@example.com", u => u.AssignRole(UserRole.Administrator));
        }

        [Test]
        public async Task ItShouldReturnAllUsers()
        {
            var userEmails = await ApiService.GetAllUsersAsync(_oc, CancellationToken.None)
                .Select(u => u.email)
                .ToListAsync();

            Assert.That(userEmails, Is.EquivalentTo(["jane.doe@example.com", "john.doe@example.com"]));
        }
    }
}

