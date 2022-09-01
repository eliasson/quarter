using NUnit.Framework;
using Quarter.Components;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Components;

[TestFixture]
public class EmptyCollectionMessageTest
{
    public class WhenRendered : TestCase
    {
        [OneTimeSetUp]
        public void SetupWhenRendered()
        {
            RenderWithParameters(builder =>
                builder
                    .Add(p => p.Header, "No projects")
                    .Add(p => p.Message, "You have not created any projects yet."));
        }

        [Test]
        public void ItHasAHeader()
            => Assert.That(Header(), Is.EqualTo("No projects"));

        [Test]
        public void ItHasAMessage()
            => Assert.That(Message(), Is.EqualTo("You have not created any projects yet."));
    }

    public class TestCase : BlazorComponentTestCase<EmptyCollectionMessage>
    {
        protected string Header()
            => TextForElement("[test=empty-header]");

        protected string Message()
            => TextForElement("[test=empty-message]");
    }
}