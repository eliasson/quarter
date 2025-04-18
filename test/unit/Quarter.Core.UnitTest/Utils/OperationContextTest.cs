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
}
