using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Utils;
using Quarter.Pages.Admin.Users;
using Quarter.State;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Admin;

[TestFixture]
public class UserTableTest
{
    public class WhenMenuItemIsSelected : TestCase
    {
        private User _userAlpha;

        [OneTimeSetUp]
        public async Task StartUp()
        {
            _userAlpha = await AddExistingUser("alpha@example.com");
            await AddExistingUser("bravo@example.com");
            Render();

            MenuItemForFirstUser("Remove user").Click();
        }

        [Test, Ignore("TODO")]
        public async Task ShouldDispatchShowRemoveUserAction()
            => Assert.True(await EventuallyDispatchedAction(new ShowRemoveUserAction(_userAlpha.Id.AsString())));
    }

    public class WhenThereAreUsers : TestCase
    {
        [OneTimeSetUp]
        public async Task StartUp()
        {
            await AddExistingUser("alpha@example.com");
            await AddExistingUser("bravo@example.com");
            Render();
        }

        [Test]
        public void ItShouldListUsers()
            => Assert.That(Users(), Is.EquivalentTo(new [] {"alpha@example.com", "bravo@example.com" }));
    }

    public class TestCase : BlazorComponentTestCase<UsersTable>
    {
        protected Task<User> AddExistingUser(string email)
        {
            var user = User.StandardUser(new Email(email));
            return RepositoryFactory.UserRepository().CreateAsync(user, CancellationToken.None);
        }

        protected IEnumerable<string> Users()
            => ComponentsByTestAttribute("user-email")?.Select(e => e.TextContent);

        protected IElement MenuItemForFirstUser(string title)
        {
            ComponentByTestAttribute("menu-launcher").Click();
            return Component?.FindAll("li").First(m => m.Attributes["test-menu"]?.Value == title);
        }
    }
}