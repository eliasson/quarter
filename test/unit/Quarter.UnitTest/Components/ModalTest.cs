using Bunit;
using NUnit.Framework;
using Quarter.Components;
using Quarter.State;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Components
{
    [TestFixture]
    public class ModalTest
    {
        public class WhenRendered : TestCase
        {
            [OneTimeSetUp]
            public void SetupWhenRendered()
            {
                RenderWithParameters(builder =>
                    builder
                        .Add(p => p.Title, "Hello test")
                        .AddChildContent("<span test=test-handle>unit-test-content</span>"));
            }

            [Test]
            public void ItRendersATitle()
            {
                var title = Title();
                Assert.That(title, Is.EqualTo("Hello test"));
            }

            [Test]
            public void ItRendersChildContent()
            {
                var childContent = TextForElement("[test=test-handle]");
                Assert.That(childContent, Does.Contain("unit-test-content"));
            }
        }

        public class WhenClosed : TestCase
        {
            [OneTimeSetUp]
            public void SetupWhenRendered()
            {
                RenderWithParameters(builder =>
                    builder
                        .AddChildContent("<span test=test-handle>unit-test-content</span>"));
                Close();
            }

            [Test]
            public void ItDispatchCloseModalAction()
                => Assert.True(DidDispatchAction(typeof(CloseModalAction)));
        }

        public class TestCase : BlazorComponentTestCase<Modal>
        {
            protected string Title()
                => Component?.Find("[test=modal-title]").TextContent;

            protected void Close()
                => Component?.Find("[test=modal-close]").Click();
        }
    }
}