using AngleSharp.Dom;
using Bunit;
using NUnit.Framework;
using Quarter.Components;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Components;

[TestFixture]
public class PageColumnTest
{
    public class WhenAllColumnsAreUsed : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb => pb
                .Add(ps => ps.ContextContent, "One")
                .Add(ps => ps.MainContent, "Two")
                .Add(ps => ps.ToolbarContent, "Three"));
        }

        [Test]
        public void ItShouldRenderContextContent()
            => Assert.That(ContextElement().TextContent, Is.EqualTo("One"));

        [Test]
        public void ItShouldRenderMainContent()
            => Assert.That(MainElement().TextContent, Is.EqualTo("Two"));

        [Test]
        public void ItShouldRenderToolbarContent()
            => Assert.That(ToolbarElement().TextContent, Is.EqualTo("Three"));
    }

    public class WhenNoColumnsAreGiven : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        => Render();

        [Test]
        public void ItDoesNotRenderContextContent()
            => Assert.Catch<ElementNotFoundException>(() => ContextElement());

        [Test]
        public void ItShouldRenderMainContent()
            => Assert.That(MainElement().TextContent, Is.Empty);

        [Test]
        public void ItDoesNotRenderToolbarContent()
            => Assert.Catch<ElementNotFoundException>(() => ToolbarElement());
    }

    public class TestCase : BlazorComponentTestCase<PageColumns>
    {
        protected IElement ContextElement()
            => Component.Find(".qa-page-content__context");

        protected IElement MainElement()
            => Component.Find(".qa-page-content__main");

        protected IElement ToolbarElement()
            => Component.Find(".qa-page-content__toolbar");
    }
}
