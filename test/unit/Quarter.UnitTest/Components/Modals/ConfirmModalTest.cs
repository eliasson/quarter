using System.Linq;
using AngleSharp.Dom;
using Bunit;
using NUnit.Framework;
using Quarter.Components.Modals;
using Quarter.State;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Components.Modals;

public abstract class ConfirmModalTest
{
    public class WhenRendered : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb => pb
                .Add(c => c.Title,  "Some title")
                .Add(c => c.Message, "Some message")
                .Add(c => c.ConfirmText, "Confirm"));
        }

        [Test]
        public void ItShouldUseTitleParameter()
            => Assert.That(Title(), Is.EqualTo("Some title"));

        [Test]
        public void ItShouldShowMessageParameter()
            => Assert.That(Message(), Is.EqualTo("Some message"));

        [Test]
        public void ItShouldHaveAConfirmButton()
            => Assert.That(ConfirmButton()?.Text(), Is.EqualTo("Confirm"));

        [Test]
        public void ItShouldHaveACancelButton()
            => Assert.That(CancelButton()?.Text(), Is.EqualTo("Cancel"));

        [Test]
        public void ItShouldShouldNotHaveDangerousConfirmAction()
            => Assert.That(ConfirmButton()?.ClassList.Contains("qa-button--danger"), Is.False);
    }

    public class WhenRenderedDangerous : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb => pb
                .Add(c => c.Title,  "Some title")
                .Add(c => c.Message, "Some message")
                .Add(c => c.ConfirmText, "Confirm")
                .Add(c => c.IsDangerous, true));
        }

        [Test]
        public void ItShouldShouldNotHaveDangerousConfirmAction()
            => Assert.That(ConfirmButton()?.ClassList.Contains("qa-button--danger"), Is.True);
    }

    public class WhenConfirming : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb => pb
                .Add(c => c.Title, "Some title")
                .Add(c => c.Message, "Some message")
                .Add(c => c.OnConfirmAction, new UnitTestAction()));
            Submit();
        }

        [Test]
        public void ItShouldDispatchOnConfirmAction()
            => Assert.IsTrue(DidDispatchAction(new UnitTestAction()));
    }

    public class WhenCancelling : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb => pb
                .Add(c => c.Title,  "Some title")
                .Add(c => c.Message, "Some message"));
            CancelButton().Click();
        }

        [Test]
        public void ItShouldDispatchCloeModalAction()
            => Assert.True(DidDispatchAction(new CloseModalAction()));
    }

    public class TestCase : BlazorModalTestCase<ConfirmModal>
    {
        protected string Message()
            => ComponentByTestAttribute("modal-message")?.TextContent;
    }
}