using System.Linq;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using NUnit.Framework;

namespace Quarter.Core.UnitTest.Models
{
    [TestFixture]
    public class UserTest
    {
        public class WhenConstructed
        {
            private readonly User _user = new User(new Email("jane.doe@example.com"));

            [Test]
            public void ItShouldGetAnIdAssigned()
                => Assert.That(_user.Id, Is.Not.Null);

            [Test]
            public void ItShouldBeCreated()
                => Assert.That(_user.Created, Is.Not.Null);

            [Test]
            public void ItShouldNotBeUpdated()
                => Assert.That(_user.Updated, Is.Null);

            [Test]
            public void ItShouldHaveNoRole()
                => Assert.That(_user.Roles, Is.Empty);
        }

        public class WithRoles
        {
            public class AsStandardUser
            {
                [Test]
                public void ItShouldHaveNoRole()
                {
                    var user = User.StandardUser(new Email("foo@example.com"));

                    Assert.That(user.Roles, Is.Empty);
                }

                [Test]
                public void ItShouldNotBeAdmin()
                {
                    var user = new User(new Email("foo@example.com"), User.NoRoles);

                    Assert.That(user.IsAdmin(), Is.False);
                }
            }

            public class AsAdminUser
            {
                [Test]
                public void ItShouldHaveAdminRole()
                {
                    var user = User.AdminUser(new Email("foo@example.com"));

                    Assert.That(user.Roles, Is.EqualTo(new [] {UserRole.Administrator}));
                }

                [Test]
                public void ItShouldBeAdmin()
                {
                    var user = new User(new Email("foo@example.com"), new [] {UserRole.Administrator}.ToList());

                    Assert.That(user.IsAdmin(), Is.True);
                }
            }

            [Test]
            public void ItShouldAssignNewRole()
            {
                var user = User.StandardUser(new Email("foo@example.com"));
                user.AssignRole(UserRole.Administrator);

                Assert.That(user.Roles, Is.EqualTo(new [] { UserRole.Administrator }));
            }

            [Test]
            public void ItShouldNotAssignDuplicateRoles()
            {
                var user = User.StandardUser(new Email("foo@example.com"));
                user.AssignRole(UserRole.Administrator);
                user.AssignRole(UserRole.Administrator);

                Assert.That(user.Roles, Is.EqualTo(new [] { UserRole.Administrator }));
            }

            [Test]
            public void ItShouldRevokeExistingRole()
            {
                var user = new User(new Email("foo@example.com"), new [] {UserRole.Administrator}.ToList());
                user.RevokeRole(UserRole.Administrator);

                Assert.That(user.Roles, Is.Empty);
            }

            [Test]
            public void ItShouldBeNoopToRevokeNotExistingRole()
            {
                var user = new User(new Email("foo@example.com"), new [] {UserRole.Administrator}.ToList());
                user.RevokeRole(UserRole.Administrator);
                user.RevokeRole(UserRole.Administrator);

                Assert.That(user.Roles, Is.Empty);
            }
        }
    }
}