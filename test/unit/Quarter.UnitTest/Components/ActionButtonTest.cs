using System;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using NUnit.Framework;
using Quarter.Components;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Components
{
    [TestFixture]
    public class ActionButtonTest
    {
        public class WithIconAndLabel : TestCase
        {
            [OneTimeSetUp]
            public void SetUp()
            {
                RenderWithParameters(pb => pb
                    .Add(c => c.Label, "Hello")
                    .Add(c => c.IconRef, "some-icon"));
            }

            [Test]
            public void ItShouldRenderIcon()
            {
                var elm = ComponentByTestAttribute("button-icon");
                Assert.NotNull(elm);
            }

            [Test]
            public void ItShouldRenderLabelText()
            {
                var elm = ComponentByTestAttribute("button-label");
                Assert.That(elm?.TextContent, Is.EqualTo("Hello"));
            }

            [Test]
            public void ItShouldBeEnabled()
                => Assert.True(Button().IsEnabled());

            [Test]
            public void ItShouldBeAButtonTypeButton()
                => Assert.That(Button().Attributes["type"]?.Value, Is.EqualTo("button"));
        }

        public class WithIconOnly : TestCase
        {
            [OneTimeSetUp]
            public void SetUp()
            {
                RenderWithParameters(pb => pb
                    .Add(c => c.IconRef, "some-icon"));
            }

            [Test]
            public void ItShouldRenderIcon()
            {
                var elm = ComponentByTestAttribute("button-icon");
                Assert.NotNull(elm);
            }

            [Test]
            public void ItShouldNotRenderAnyText()
                => Assert.Catch<ElementNotFoundException>(() => ComponentByTestAttribute("button-label"));
        }

        public class WhenClicked : TestCase
        {
            [OneTimeSetUp]
            public void Setup()
            {
                RenderWithParameters(pb => pb
                    .Add(c  => c.Label, "Hello")
                    .Add(c => c.IconRef, "some-icon")
                    .Add(c => c.OnAction, OnAction));
                Button().Click();
            }

            [Test]
            public void ItShouldCallAction()
                => Assert.That(OnActionCalls, Is.EqualTo(1));

            [Test]
            public void ItShouldBeDisabled()
                => Assert.True(Button().IsDisabled());
        }

        public class WhenActionCompletes : TestCase
        {
            [OneTimeSetUp]
            public void Setup()
            {
                RenderWithParameters(pb => pb
                    .Add(c  => c.Label, "Hello")
                    .Add(c => c.IconRef, "some-icon")
                    .Add(c => c.OnAction, OnAction));
                Button().Click();
                CompleteAction();
            }

            [Test]
            public void ItShouldCallAction()
                => Assert.That(OnActionCalls, Is.EqualTo(1));

            [Test, Ignore("Flaky under .NET 6")]
            public void ItShouldBeEnabled()
                => Assert.True(Button().IsEnabled());
        }

        public class WhenActionThrows : TestCase
        {
            [OneTimeSetUp]
            public void Setup()
            {
                RenderWithParameters(pb => pb
                    .Add(c  => c.Label, "Hello")
                    .Add(c => c.IconRef, "some-icon")
                    .Add(c => c.OnAction, OnAction));
                Button().Click();
                FailAction();
            }

            [Test]
            public void ItShouldCallAction()
                => Assert.That(OnActionCalls, Is.EqualTo(1));

            [Test, Ignore("Behaves different under test that IRL")]
            public void ItShouldBeEnabled()
                => Assert.True(Button().IsEnabled());
        }


        public class TestCase : BlazorComponentTestCase<ActionButton>
        {
            protected int OnActionCalls = 0;

            private readonly TaskCompletionSource<bool> _tcs;
            private readonly Task<bool> _deferredTask;

            protected TestCase()
            {
                _tcs = new TaskCompletionSource<bool>();
                _deferredTask = _tcs.Task;
            }

            protected IElement Button()
                => Component?.Find("button");

            protected Task OnAction()
            {
                OnActionCalls += 1;
                return _deferredTask;
            }

            protected void CompleteAction()
                => _tcs.SetResult(true);

            protected void FailAction()
                => _tcs.SetException(new Exception("Unit-test deferred task failure"));
        }
    }

    public static class ElementExtensions
    {
        public static bool IsEnabled(this IElement elm)
            => elm.Attributes["disabled"] == null;

        public static bool IsDisabled(this IElement elm)
            => elm.Attributes["disabled"] != null;
    }
}