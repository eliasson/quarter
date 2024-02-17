using System;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Quarter.Core.Queries;
using Quarter.Core.Utils;
using Quarter.Pages.Application.Timesheet;
using Quarter.State;
using Quarter.UnitTest.TestUtils;
using TestContext = Bunit.TestContext;

namespace Quarter.UnitTest.Pages.Application.TimesheetPage;

[TestFixture]
public class TimesheetPageTest
{
    public class WhenUrlParameterIsMissing : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Render();
        }

        [Test]
        public void ItShouldSetTheCurrentTimesheetDate()
            => Assert.That(SelectedDate(), Is.EqualTo(DateTime.UtcNow.Date));
    }

    public class WhenRenderedWithDateParameter : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            RenderWithParameters(pb =>
                pb.Add(ps => ps.SelectedDate, TestDate));
        }

        [Test]
        public void ItShouldSetTheGivenTimesheetDate()
            => Assert.That(SelectedDate(), Is.EqualTo(TestDate));

        [Test]
        public void ItShouldDispatchActionToLoadProjects()
            => Assert.That(DidDispatchAction(new LoadProjects()), Is.True);

        [Test]
        public void ItShouldDispatchActionToLoadTimesheet()
            => Assert.That(DidDispatchAction(new LoadTimesheetAction(new Date(TestDate))), Is.True);

        [Test]
        public void ItShouldHaveATimesheetSummaryWidget()
            => Assert.DoesNotThrow(() => Component?.FindComponent<TimesheetSummaryWidget>());

        [Test]
        public void ItShouldHaveATimesheetActivitySelectorWidget()
            => Assert.DoesNotThrow(() => Component?.FindComponent<TimesheetActivitySelectorWidget>());

        [Test]
        public void ItShouldHaveATimesheetGrid()
            => Assert.DoesNotThrow(() => Component?.FindComponent<TimesheetGrid>());
    }

    public abstract class TestCase : BlazorComponentTestCase<Quarter.Pages.Application.Timesheet.TimesheetPage>
    {
        protected readonly DateTime TestDate = DateTime.UtcNow.Date.AddDays(1); // Not today is the only important criteria

        private readonly TestQueryHandler _queryHandler = new ();

        protected override void ConfigureTestContext(TestContext ctx)
        {
            base.ConfigureTestContext(ctx);
            Context.Services.AddSingleton<IQueryHandler>(_queryHandler);
        }

        protected DateTime? SelectedDate()
            => Component?.Instance.SelectedDate;
    }
}
