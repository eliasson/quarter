using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Utils;

[TestFixture]
public class OperationContextTest
{
    [Test]
    public void ItShouldBeNone()
    {
        var oc = OperationContext.None;
        Assert.That(oc.IsNone, Is.True);
    }

    [Test]
    public void ItShouldBeEqual()
    {
        var userId = IdOf<User>.Random();

        var ocOne = new OperationContext(userId, [UserRole.Administrator]);
        var ocTwo = new OperationContext(userId, [UserRole.Administrator]);

        Assert.That(ocOne, Is.EqualTo(ocTwo));
    }

    [TestCase(UserRole.Administrator, true)]
    [TestCase(UserRole.Administrator, false)]
    public void ItShouldHaveRole(UserRole role, bool expected)
    {
        var oc = expected
            ? new OperationContext(IdOf<User>.Random(), [UserRole.Administrator])
            : new OperationContext(IdOf<User>.Random(), []);

        Assert.That(oc.HasRole(role), Is.EqualTo(expected));
    }
}
