using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Utils;

public class OperationContextTest
{
    [Test]
    public void ItShouldBeNone()
    {
        var oc = OperationContext.None;
        Assert.True(oc.IsNone);
    }

    [Test]
    public void ItShouldBeEqual()
    {
        var userId = IdOf<User>.Random();
        var ocOne = new OperationContext(userId);
        var ocTwo = new OperationContext(userId);

        Assert.That(ocOne, Is.EqualTo(ocTwo));
    }
}