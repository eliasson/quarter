using System.Collections.Generic;
using NUnit.Framework;
using Quarter.Components.Navigation;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Components.Navigation
{
    [TestFixture]
    public class AdminNavigationTest
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
                yield return new object[] { "nav-admin", Page.Admin, "Admin" };
                yield return new object[] { "nav-users", Page.AdminUsers, "Users" };
                yield return new object[] { "nav-home", Page.Home, "Application" };
                yield return new object[] { "nav-logout", Page.Logout, "Logout" };
            }

            [TestCaseSource(nameof(NavigationTests))]
            public void ItShouldNavigateToDestination(string selector, string expectedDestination, string _)
            {
                var anchor = ComponentByTestAttribute(selector);

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
        }

        public class TestCase : BlazorComponentTestCase<AdminPageNavigation>
        {
        }
    }
}
