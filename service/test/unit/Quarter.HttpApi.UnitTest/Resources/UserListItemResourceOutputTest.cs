using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Resources;

[TestFixture]
public class UserListItemResourceOutputTest
{
    [TestFixture]
    public class WhenConstructingMinimalUserOutput
    {
        private User _user = null!;
        private UserListItemResourceOutput _output = null!;

        [OneTimeSetUp]
        public void Setup()
        {
            _user = new User(new Email("jane.doe@example.com"));
            _output = UserListItemResourceOutput.From(_user, lastLogin: null);
        }

        [Test]
        public void ItShouldMapId()
            => Assert.That(_output.id, Is.EqualTo(_user.Id.Id.ToString()));

        [Test]
        public void ItShouldMapEmail()
            => Assert.That(_output.email, Is.EqualTo(_user.Email.Value));

        [Test]
        public void ItShouldMapCreatedTimestamp()
            => Assert.That(_output.created, Is.EqualTo(_user.Created.IsoString()));

        [Test]
        public void ItShouldLackUpdateTimestamp()
            => Assert.That(_output.updated, Is.Null);

        [Test]
        public void ItShouldHaveNoRoles()
            => Assert.That(_output.roles, Is.Empty);

        [Test]
        public void ItShouldLackLastLogin()
            => Assert.That(_output.lastLogin, Is.Null);
    }

    [TestFixture]
    public class WhenUserHasLoggedIn
    {
        private readonly UtcDateTime _lastLogin = UtcDateTime.Now();
        private UserListItemResourceOutput _output = null!;

        [OneTimeSetUp]
        public void Setup()
            => _output = UserListItemResourceOutput.From(new User(new Email("jane.doe@example.com")), _lastLogin);

        [Test]
        public void ItShouldMapLastLoginAsIsoTimestamp()
            => Assert.That(_output.lastLogin, Is.EqualTo(_lastLogin.IsoString()));
    }

    [TestFixture]
    public class WhenConstructingAdminUserOutput
    {
        private UserListItemResourceOutput _output = null!;

        [OneTimeSetUp]
        public void Setup()
            => _output = UserListItemResourceOutput.From(User.AdminUser(new Email("jane.doe@example.com")), lastLogin: null);

        [Test]
        public void ItShouldMapAdministratorRole()
            => Assert.That(_output.roles, Is.EqualTo(["administrator"]));
    }
}
