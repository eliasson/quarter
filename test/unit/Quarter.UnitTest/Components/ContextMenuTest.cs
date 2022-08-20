using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;
using Bunit;
using NUnit.Framework;
using Quarter.Components;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Components;

[TestFixture]
public class ContextMenuTest
{
     private static readonly List<ContextMenu.MenuItemVm> TestItems = new List<ContextMenu.MenuItemVm>
        {
            new("one", "One"),
            new("two", "Two"),
        };

        public class WhenCollapsed : TestCase
        {
            [OneTimeSetUp]
            public void Setup()
                => RenderWithParameters(pb =>
                {
                    pb.Add(p => p.Items, TestItems);
                    pb.Add(p => p.ItemSelected, () => { });
                });

            [Test]
            public void ItShouldRenderMenuLauncher()
                => Assert.That(Launcher(), Is.Not.Null);

            [Test]
            public void ItShouldNotRenderTheMenu()
                => Assert.Catch<ElementNotFoundException>(() => Menu());

            [Test]
            public void ItShouldRenderMenuLauncherNormally()
            {
                var classes = Launcher()?.ClassList.Select(c => c);
                Assert.That(classes, Does.Not.Contain("qa-button--inverted"));
            }
        }

        public class WhenOpened : TestCase
        {
            [OneTimeSetUp]
            public void Setup()
            {
                RenderWithParameters(pb =>
                {
                    pb.Add(p => p.Items, TestItems);
                    pb.Add(p => p.ItemSelected, (_) => { });
                });
                Launcher()?.Click();
            }

            [Test]
            public void ItShouldStillRenderMenuLauncher()
                => Assert.That(Launcher(), Is.Not.Null);

            [Test]
            public void ItShouldRenderMenuItems()
            {
                var items = MenuItems();
                var labels = items?.Select(elm => elm.QuerySelector("[test=menu-item-label]").TextContent);
                Assert.That(labels, Is.EqualTo(new [] { "One", "Two" }));
            }
        }

        public class WhenSelectingAnItem : TestCase
        {
            private readonly List<ContextMenu.MenuItemVm> _selectedItems = new ();

            [OneTimeSetUp]
            public void Setup()
            {
                RenderWithParameters(pb =>
                {
                    pb.Add(p => p.Items, TestItems);
                    pb.Add(p => p.ItemSelected, (item) => _selectedItems.Add(item));
                });
                Launcher()?.Click();
                var menuItem = MenuItems()?[1];
                menuItem?.Click();
            }

            [Test]
            public void ItShouldNoLongerRenderTheMenu()
                => Assert.Catch<ElementNotFoundException>(() => Menu());

            [Test]
            public void ItShouldTriggerEvent()
                => Assert.That(_selectedItems, Is.EqualTo(new []
                {
                    new ContextMenu.MenuItemVm("two", "Two")
                }));
        }

        [Ignore("issue#14")]
        public class WhenClickingOutsideMenu : TestCase
        {
            [OneTimeSetUp]
            public void Setup()
            {
                RenderWithParameters(pb =>
                {
                    pb.Add(p => p.Items, TestItems);
                    pb.Add(p => p.ItemSelected, (_) => {});
                });
                Launcher()?.Click();
                ElementOutsideMenu()?.Click();
            }

            [Test]
            public void ItShouldNoLongerRenderTheMenu()
                => Assert.Catch<ElementNotFoundException>(() => Menu());
        }

        public class WhenInverted : TestCase
        {
            [OneTimeSetUp]
            public void Setup()
                => RenderWithParameters(pb =>
                {
                    pb.Add(p => p.Items, TestItems);
                    pb.Add(p => p.ItemSelected, () => { });
                    pb.Add(p => p.IsInverted, true);
                });

            [Test]
            public void ItShouldRenderMenuLauncherInverted()
            {
                var classes = Launcher()?.ClassList.Select(c => c);
                Assert.That(classes, Does.Contain("qa-button--inverted"));
            }

            [Test]
            public void ItShouldNotRenderTheMenu()
                => Assert.Catch<ElementNotFoundException>(() => Menu());
        }

        public class TestCase : BlazorComponentTestCase<ContextMenu>
        {
            protected IElement Launcher()
                => ComponentByTestAttribute("menu-launcher");

            protected IElement Menu()
                => Component?.Find("menu");

            protected IElement ElementOutsideMenu()
                => ComponentByTestAttribute("menu-backdrop");

            protected IRefreshableElementCollection<IElement> MenuItems()
                => ComponentsByTestAttribute("menu-item");
        }
}