using System;
using System.Collections.Generic;
using System.Linq;
using Bunit;
using NUnit.Framework;
using Quarter.Components;
using Quarter.UnitTest.TestUtils;
using Quarter.Utils;

namespace Quarter.UnitTest.Components
{
    [TestFixture]
    public class PageContextTest
    {
        public class WhenRendered : TestCase
        {
            [OneTimeSetUp]
            public void SetupWhenRendered()
            {
                var tabs = new List<TabData>
                {
                    new TabData("Foo", "/foo"),
                    new TabData("Bar", "/bar")
                };

                RenderWithParameters(pb => pb
                    .Add(c => c.Tabs, tabs)
                    .Add(c => c.ChildContent, "<div test=\"child-content\">foo</div>")
                );
            }

            [Test]
            public void ItShouldHaveTabs()
            {
                Assert.That(Tabs(), Is.EquivalentTo(new[]
                {
                    ("Foo", "/foo"),
                    ("Bar", "/bar")
                }));
            }

            [Test]
            public void ItShouldRenderChildContent()
                => Assert.DoesNotThrow(() => ComponentByTestAttribute("child-content"));
        }

        public class TestCase : BlazorComponentTestCase<PageContext>
        {
            protected IEnumerable<(string, string)> Tabs()
                => Component?.FindAll("[test=tab-item]")
                    .Select(tab => (tab.TextContent, tab.Attributes["href"]?.Value)) ?? Array.Empty<(string, string)>();
        }
    }
}