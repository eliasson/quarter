using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.HttpApi.Resources;

namespace Quarter.HttpApi.UnitTest.Resources;

[TestFixture]
public class UserResourceOutputTest
{
    [TestFixture]
    public class WhenConstructingMinimalUserOutput
    {
        private User _user = null!;
        private UserResourceOutput _output = null!;

        [OneTimeSetUp]
        public void Setup()
        {
            _user = new User(new Email("jane.doe@example.com"));
            _output = UserResourceOutput.From(_user);
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
    }
}

