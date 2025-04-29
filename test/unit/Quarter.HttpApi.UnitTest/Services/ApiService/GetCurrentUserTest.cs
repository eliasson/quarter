using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Utils;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class GetCurrentUserTest
{
    [TestFixture]
    public class Default : TestCase
    {
        private  OperationContext _oc = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var user = await AddUser("jane.doe@example.com");
            _oc = CreateOperationContext(user);
        }

        [Test]
        public async Task ItShouldReturnCurrentUser()
        {
            var user = await ApiService.GetCurrentUserAsync(_oc, CancellationToken.None);

            Assert.That(user.email, Is.EqualTo("jane.doe@example.com"));
        }
    }
}

