using Quarter.Core.Models;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.TestUtils
{
    public class AnotherTestAggregate : IAggregate<AnotherTestAggregate>
    {
        public IdOf<AnotherTestAggregate> Id { get; } = IdOf<AnotherTestAggregate>.Random();
        public UtcDateTime Created { get; } = UtcDateTime.Now();
        public UtcDateTime? Updated { get; set; } = UtcDateTime.Now();
    }
}
