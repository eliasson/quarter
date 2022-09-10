using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Quarter.Core.Queries;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Queries;

[TestFixture]
public class MonthlyReportQueryTest
{
    public class WhenNoTimeIsRegistered : QueryTestBase
    {
        private static readonly Date Today = Date.Today();
        private MonthlyReportResult _result;

        [OneTimeSetUp]
        public async Task Setup()
            => _result = await QueryHandler.ExecuteAsync(new MonthlyReportQuery(Today), OperationContext(),
                CancellationToken.None);

        [Test]
        public void ItShouldContainStartOfMonth()
            => Assert.That(_result.StartOfMonth, Is.EqualTo(Today.StartOfMonth()));

        [Test]
        public void ItShouldContainEndOfMonth()
            => Assert.That(_result.EndOfMonth, Is.EqualTo(Today.EndOfMonth()));

        [Test]
        public void ItShouldContainTotalMinutes()
            => Assert.That(_result.TotalMinutes, Is.EqualTo(0));

        [Test]
        public void ItShouldContainTotalHours()
            => Assert.That(_result.TotalMinutes.MinutesAsHours(), Is.EqualTo("0.00"));
    }
}