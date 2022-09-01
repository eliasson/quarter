using System.Linq;
using System.Security.Claims;
using NUnit.Framework;
using Quarter.Services;

namespace Quarter.UnitTest.Services;

[TestFixture]
public class AuthorizedResultTest
{
    public class AnUnauthorizedUser
    {
        private readonly AuthorizedResult _result = AuthorizedResult.Unauthorized();

        [Test]
        public void ItShouldNotBeAuthorized()
            => Assert.That(_result.State, Is.EqualTo(AuthorizedState.NotAuthorized));

        [Test]
        public void ItShouldHaveNoClaims()
            => Assert.That(_result.Claims, Is.Empty);
    }

    public class AnAuthorizedUser
    {
        private readonly AuthorizedResult _result = AuthorizedResult.AuthorizedWith(
            new Claim("A", "a"),
            new Claim("B", "b"));

        [Test]
        public void ItShouldBeAuthorized()
            => Assert.That(_result.State, Is.EqualTo(AuthorizedState.Authorized));

        [Test]
        public void ItShouldHaveGivenClaims()
        {
            var claims = _result.Claims.Select(c => c.Value);
            Assert.That(claims, Is.EquivalentTo(new [] {"a", "b"}));
        }
    }
}