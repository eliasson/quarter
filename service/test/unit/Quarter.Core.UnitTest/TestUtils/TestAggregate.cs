using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.TestUtils
{
    public class TestAggregate : IAggregate<TestAggregate>
    {
        public IdOf<TestAggregate> Id { get; } = IdOf<TestAggregate>.Random();
        public UtcDateTime Created { get; } = UtcDateTime.Now();
        public UtcDateTime? Updated { get; set; } = UtcDateTime.Now();
    }
}
