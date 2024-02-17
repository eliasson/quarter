using System;
using Quarter.Core.Utils;
using NUnit.Framework;

namespace Quarter.Core.UnitTest.Utils
{
    [TestFixture]
    public class EmailTest
    {
        [TestCase("")]
        [TestCase("bobby-tables.com")]
        [TestCase("little bobby@example.com")]
        public void ItShouldFailWhenNotValid(string value)
            => Assert.Throws<ArgumentException>(() => new Email(value));

        [TestCase("okstas@example.com")]
        [TestCase("a@b.c")]
        public void ItShouldSucceedForValidAddress(string value)
        {
            var email = new Email(value);
            Assert.That(email.Value, Is.EqualTo(value));
        }

        [TestCase("Ok@EXAMPLE.com", "ok@example.com")]
        [TestCase("A@B.C", "a@b.c")]
        public void ItShouldConvertToLowerCase(string value, string expected)
        {
            var email = new Email(value);
            Assert.That(email.Value, Is.EqualTo(expected));
        }

        [TestCase("one@example.com", "one@example.com", true)]
        [TestCase("one@example.com", "one@EXAMPLE.com", true)]
        [TestCase("one@example.com", "two@example.com", false)]
        public void ItShouldBeEqual(string one, string two, bool expectedEqual)
        {
            if (expectedEqual)
                Assert.That(new Email(one), Is.EqualTo(new Email(two)));
            else
                Assert.That(new Email(one), Is.Not.EqualTo(new Email(two)));
        }

        [Test]
        public void ItShouldUseEmailAsString()
            => Assert.That(new Email("one@example.com").AsString(), Is.EqualTo("one@example.com"));
    }
}
