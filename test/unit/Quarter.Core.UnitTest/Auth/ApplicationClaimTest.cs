using System.Security.Claims;
using NUnit.Framework;
using Quarter.Core.Auth;
using Quarter.Core.Models;

namespace Quarter.Core.UnitTest.Auth;

[TestFixture]
public class ApplicationClaimTest
{
    private Claim _claim;
    private IdOf<User> _userId;

    [OneTimeSetUp]
    public void Setup()
    {
        _userId = IdOf<User>.Random();
        _claim = ApplicationClaim.FromUserId(_userId);
    }

    [Test]
    public void ItShouldHaveUserIdAsValue()
        => Assert.That(_claim.Value, Is.EqualTo(_userId.AsString()));

    [Test]
    public void ItShouldHaveExpectedType()
        => Assert.That(_claim.Type, Is.EqualTo("quarter-user-id"));
}
