using NUnit.Framework;
using Quarter.Pages.Admin.Metrics;
using Quarter.UnitTest.TestUtils;

namespace Quarter.UnitTest.Pages.Admin.Metrics;

[TestFixture]
public class AdminMetricTest
{
    [TestFixture]
    public class Default : TestCase
    {
        [OneTimeSetUp]
        public void StartUp()
        {
            RenderWithParameters(b => b
                .Add(p => p.Value, 12)
                .Add(p => p.Unit, "Users"));
        }

        [Test]
        public void ItShouldUseTheGivenValue()
            => Assert.That(Value(), Is.EqualTo("12"));

        [Test]
        public void ItShouldUseTheGivenUnit()
            => Assert.That(Unit(), Is.EqualTo("Users"));
    }

    public abstract class TestCase : BlazorComponentTestCase<AdminMetric>
    {
        protected string Value()
            => ComponentByTestAttribute("metric-value").TextContent;

        protected string Unit()
            => ComponentByTestAttribute("metric-unit").TextContent;
    }
}
