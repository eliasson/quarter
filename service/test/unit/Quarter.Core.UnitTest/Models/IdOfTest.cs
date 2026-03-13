using System;
using System.Collections.Generic;
using System.Linq;
using Quarter.Core.Models;
using Quarter.Core.UnitTest.TestUtils;
using NUnit.Framework;

namespace Quarter.Core.UnitTest.Models
{
    [TestFixture]
    public class IdOfTest
    {
        [Test]
        public void CanBeConstructedFromGuid()
        {
            var guid = Guid.NewGuid();
            var id = IdOf<TestAggregate>.Of(guid);

            Assert.That(id.Id, Is.EqualTo(guid));
        }

        [Test]
        public void CanBeConstructedFromString()
        {
            var guid = Guid.NewGuid();
            var id = IdOf<TestAggregate>.Of(guid.ToString());

            Assert.That(id.Id, Is.EqualTo(guid));
        }

        [Test]
        public void ThrowsWhenConstructingFromInvalidString()
        {
            Assert.Multiple(() =>
            {
                var ex = Assert.Catch<ArgumentException>(() => IdOf<TestAggregate>.Of("Invalid"));
                Assert.That(ex?.Message, Is.EqualTo("Invalid is not a valid UUID"));
            });
        }

        [Test]
        public void IsRandom()
        {
            const int count = 10;
            var allocated = new HashSet<Guid>();
            foreach (var _ in Enumerable.Range(0, 10))
                allocated.Add(IdOf<TestAggregate>.Random().Id);

            Assert.That(allocated.Count, Is.EqualTo(count));
        }

        [Test]
        public void AreEqual()
        {
            var id = IdOf<TestAggregate>.Random();
            var id2 = IdOf<TestAggregate>.Of(id.Id);

            Assert.That(id2, Is.EqualTo(id));
        }

        [Test]
        public void CanCompare()
        {
            var id = IdOf<TestAggregate>.Random();
            var id2 = IdOf<TestAggregate>.Of(id.Id);

            Assert.That(id2 == id, Is.True);
        }

        [Test]
        public void CanBeRetrieveAsString()
        {
            var guid = Guid.NewGuid();
            var id = IdOf<TestAggregate>.Of(guid);

            Assert.That(id.AsString(), Is.EqualTo(guid.ToString()));
        }

        [Test]
        public void TwoIdsForDifferentAggregatesWithSameIdAreNotEqual()
        {
            var guid = Guid.NewGuid();
            var idForOneType = IdOf<TestAggregate>.Of(guid);
            var idForOtherType = IdOf<AnotherTestAggregate>.Of(guid);

            Assert.That(idForOneType, Is.Not.EqualTo(idForOtherType));
        }
    }
}
