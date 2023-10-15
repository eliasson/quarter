using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Quarter.Core.Options;
using Quarter.Pages.Admin.Settings;
using Quarter.Services;
using Quarter.UnitTest.TestUtils;
using TestContext = Bunit.TestContext;

namespace Quarter.UnitTest.Pages.Admin.Settings;

[TestFixture]
public class UserRegistrationTest
{
    [TestFixture]
    public class WhenUserRegistrationIsOpen : TestCase
    {
        [OneTimeSetUp]
        public void StartUp()
        {
            SetOpenRegistration(openRegistration: true);
            Render();
        }

        [Test]
        public void ItShouldUseTheGivenTitle()
            => Assert.That(Title(), Is.EqualTo("User registration is open"));

        [Test]
        public void ItShouldUseTheGivenMessage()
            => Assert.That(Message(), Does.Contain("registration is open"));
    }

    [TestFixture]
    public class WhenUserRegistrationIsNotOpen  : TestCase
    {
        [OneTimeSetUp]
        public void StartUp()
        {
            SetOpenRegistration(openRegistration: false);
            Render();
        }

        [Test]
        public void ItShouldUseTheGivenTitle()
            => Assert.That(Title(), Is.EqualTo("User registration is closed"));

        [Test]
        public void ItShouldUseTheGivenMessage()
            => Assert.That(Message(), Does.Contain("registration is closed"));
    }

    public abstract class TestCase : BlazorComponentTestCase<UserRegistration>
    {
        private readonly IOptions<AuthOptions> _authOptions = Options.Create(new AuthOptions { OpenUserRegistration = false });

        protected void SetOpenRegistration(bool openRegistration)
        {
            // _authOptions = Options.Create(new AuthOptions { OpenRegistration = openRegistration });
            _authOptions.Value.OpenUserRegistration = openRegistration;
        }

        protected override void ConfigureTestContext(TestContext ctx)
        {
            base.ConfigureTestContext(ctx);
            var adminService = new AdminService(_authOptions, RepositoryFactory);
            Context.Services.AddSingleton<IAdminService>(adminService);
        }

        protected string Title()
            => ComponentByTestAttribute("admin-setting-title").TextContent;

        protected string Message()
            => ComponentByTestAttribute("admin-setting-message").TextContent;
    }
}