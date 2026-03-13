using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Quarter.Core.Queries;
using Quarter.Pages.Application.Home;
using Quarter.Services;
using Quarter.UnitTest.TestUtils;
using TestContext = Bunit.TestContext;

namespace Quarter.UnitTest.Pages.Application.Home;

[TestFixture]
public class HomePageTest
{
    public class WhenNoTimeRegistered : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithEmptyResult();
        }

        [Test]
        public void ItShouldDisplayGreetingWidget()
            => Assert.DoesNotThrow(() => Component?.FindComponent<GreetingWidget>());

        [Test]
        public void ItShouldDisplayCurrentWeekWidget()
            => Assert.DoesNotThrow(() => Component?.FindComponent<CurrentWeekWidget>());

        [Test]
        public void ItShouldDisplayCurrentWeekTotalWidget()
            => Assert.DoesNotThrow(() => Component?.FindComponent<WeekTotalWidget>());
    }

    public abstract class TestCase : BlazorComponentTestCase<HomePage>
    {
        private readonly TestQueryHandler _queryHandler = new TestQueryHandler();
        private readonly IUserAuthorizationService _authService = new TestIUserAuthorizationService();

        protected override void ConfigureTestContext(TestContext ctx)
        {
            base.ConfigureTestContext(ctx);
            Context.Services.AddSingleton<IQueryHandler>(_queryHandler);
            Context.Services.AddSingleton(_authService);
        }

        protected void RenderWithEmptyResult()
        {
            _queryHandler.FakeTimesheetSummaryQueryResult = new TimesheetSummaryQueryResult();
            Render();
        }
    }
}
