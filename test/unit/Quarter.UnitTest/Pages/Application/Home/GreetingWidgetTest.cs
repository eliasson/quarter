using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Quarter.Pages.Application.Home;
using Quarter.Services;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Application.Home;

[TestFixture]
public class GreetingWidgetTest
{
    [TestFixture]
    public class Default : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Render();
        }

        [Test]
        public void ItShouldGreetTheCurrentUser()
            => Assert.That(Greeting(), Is.EqualTo("Hello, Jane Doe!"));
    }

    public abstract class TestCase : BlazorComponentTestCase<GreetingWidget>
    {
        private readonly IUserAuthorizationService _authService = new TestIUserAuthorizationService
        {
            Username = "Jane Doe"
        };

        protected override void ConfigureTestContext(Bunit.TestContext ctx)
        {
            base.ConfigureTestContext(ctx);
            Context.Services.AddSingleton(_authService);
        }

        protected string Greeting()
            => ComponentByTestAttribute("greeting")?.TextContent;
    }
}