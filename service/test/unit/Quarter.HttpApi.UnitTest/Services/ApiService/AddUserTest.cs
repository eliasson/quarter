using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Utils;

namespace Quarter.HttpApi.UnitTest.Services;

[TestFixture]
public class AddUserTest
{
    [TestFixture]
    public class WhenUserIsAdmin : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();
        private UserResourceOutput _userResourceOutput = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            var input = new CreateUserResourceInput { email = "new@example.com" };
            _userResourceOutput = await ApiService.AddUserAsync(input, _oc, CancellationToken.None);
        }

        [Test]
        public void ItShouldReturnTheUserResource()
            => Assert.That(_userResourceOutput.email, Is.EqualTo("new@example.com"));

        [Test]
        public async Task ItShouldHaveAddedTheUserWithoutAnyRoles()
        {
            var user = await ReadUserAsync("new@example.com");

            Assert.That(user.Roles, Is.Empty);
        }
    }

    [TestFixture]
    public class WhenInputIsInvalid : TestCase
    {
        private readonly OperationContext _oc = CreateOperationContext();

        [OneTimeSetUp]
        public async Task Setup()
        {
            await AddUser("alice@example.com");
        }

        [Test]
        public void ItShouldFailIfEmailIsNotUnique()
        {
            var input = new CreateUserResourceInput { email = "alice@example.com" };

            Assert.Multiple(() =>
            {
                var ex = Assert.CatchAsync<ArgumentException>(() => ApiService.AddUserAsync(input, _oc, CancellationToken.None));
                Assert.That(ex?.Message, Is.EqualTo("Could not store as Email already in use"));
            });
        }
    }
}
