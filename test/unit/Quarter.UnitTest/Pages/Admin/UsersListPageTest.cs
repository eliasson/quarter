using Bunit;
using NUnit.Framework;
using Quarter.Pages.Admin.Users;
using Quarter.State;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Admin
{
    [TestFixture]
    public class UsersListPageTest
    {
        public class WhenClickingAddUserButton : TestCase
        {
            [OneTimeSetUp]
            public void Setup()
            {
                Render();
                ClickActionButton();
            }

            [Test]
            public void ItShouldDispatchAction()
                => Assert.That(DidDispatchAction(new ShowAddUserAction()), Is.True);
        }

        public class TestCase : BlazorComponentTestCase<UsersListPage>
        {
            protected void ClickActionButton()
                => ComponentByTestAttribute("action-button")?.Click();
        }
    }
}
