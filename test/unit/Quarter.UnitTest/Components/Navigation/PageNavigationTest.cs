using System.Collections.Generic;
using Bunit;
using NUnit.Framework;
using Quarter.Components.Navigation;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Components.Navigation
{
    [TestFixture]
    public class PageNavigationTest
    {
        public class WhenNavigating : TestCase
        {
            [OneTimeSetUp]
            public void Setup()
            {
                Render();
            }

            public static IEnumerable<object[]> NavigationTests()
            {
                yield return new object[]{ "nav-home", Page.Home, "Home" };
                yield return new object[]{ "nav-timesheets", Page.TimesheetBase, "Timesheets" };
                yield return new object[]{ "nav-manage", Page.Manage, "Manage" };
                yield return new object[]{ "nav-report", Page.WeeklyReport, "Report" };
                yield return new object[]{ "nav-logout", Page.Logout, "Logout" };
            }

            [TestCaseSource(nameof(NavigationTests))]
            public void ItShouldNavigateToDestination(string selector, string expectedDestination, string _)
            {
                var elm = ComponentByTestAttribute(selector);
                var anchor = elm?.QuerySelector("a");

                // NOTE:
                // Currently bunit cannot invoke a click on href, so there is no way to test that
                // a navigation actually occured. Settle with asserting the href attribute instead.
                Assert.That(anchor?.Attributes["href"]?.Value, Is.EqualTo(expectedDestination));
            }

            [TestCaseSource(nameof(NavigationTests))]
            public void ItShouldHaveTitle(string selector, string _, string expectedTitle)
            {
                var elm = ComponentByTestAttribute(selector);
                var title = elm?.QuerySelector("[test=nav-title]");

                Assert.That(title?.TextContent, Is.EqualTo(expectedTitle));
            }


            [TestCase("nav-home", true)]
            [TestCase("nav-timesheets", true)]
            [TestCase("nav-manage", true)]
            [TestCase("nav-admin", false)]
            [TestCase("nav-logout", true)]
            public void ItShouldHaveNavigationItem(string selector, bool expected)
            {
                if (expected)
                    Assert.That(ComponentByTestAttribute(selector), Is.Not.Null);
                else
                    Assert.Throws<ElementNotFoundException>(() => ComponentByTestAttribute(selector));
            }
        }

        public class WhenUserIsAdmin : TestCase
        {
            [OneTimeSetUp]
            public void Setup()
            {
                SetUserIsAdmin();
                Render();
            }

            [TestCase("nav-home", true)]
            [TestCase("nav-timesheets", true)]
            [TestCase("nav-manage", true)]
            [TestCase("nav-admin", true)]
            [TestCase("nav-logout", true)]
            public void ItShouldHaveNavigationItem(string selector, bool expected)
            {
                if (expected)
                    Assert.That(ComponentByTestAttribute(selector), Is.Not.Null);
                else
                    Assert.Throws<ElementNotFoundException>(() => ComponentByTestAttribute(selector));
            }
        }

        public class TestCase : BlazorComponentTestCase<PageNavigation>
        {
        }
    }
}