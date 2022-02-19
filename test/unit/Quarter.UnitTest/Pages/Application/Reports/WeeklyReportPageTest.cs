using Bunit;
using NUnit.Framework;
using Quarter.Components;
using Quarter.Pages.Application.Reports;
using Quarter.UnitTest.TestUtils;
using Quarter.Utils;
namespace Quarter.UnitTest.Pages.Application.Reports;

public abstract class WeeklyReportPageTest
{
    [TestFixture]
    public class WhenRenderedEmpty : TestCase
    {
        [OneTimeSetUp]
        public void Setup()
            => Render();

        [Test]
        public void ItHasAProjectTab()
        {
            var context = Component?.FindComponent<PageContext>();

            Assert.That(context?.Instance.Tabs, Is.EquivalentTo(new[]
                {
                    new TabData("Weekly", Page.Report)
                }
            ));
        }
    }

    public class TestCase : BlazorComponentTestCase<WeeklyReportPage>
    {
    }
}