using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using NUnit.Framework;
using Quarter.Core.Models;
using Quarter.Core.Queries;
using Quarter.Core.Utils;
using Quarter.Pages.Application.Home;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Application.Home;

[TestFixture]
public class WeekTotalWidgetTest
{
    public class Default : TestCase
    {
        private TimesheetSummaryQueryResult _summary;

        [OneTimeSetUp]
        public void Setup()
        {
            _summary = new TimesheetSummaryQueryResult { TotalMinutes = 90 };
            RenderWithParameters(pb =>
            {
                pb.Add(ps => ps.Summary, _summary);
            });
        }

        [Test]
        public void ItShouldDisplayTotal()
            => Assert.That(Total(), Is.EqualTo("1.50"));
    }

    public class TestCase : BlazorComponentTestCase<WeekTotalWidget>
    {
        protected string Total()
            => ComponentByTestAttribute("week-total")?.TextContent;
    }
}