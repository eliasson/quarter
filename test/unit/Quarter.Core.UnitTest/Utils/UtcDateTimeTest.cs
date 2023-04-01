using System;
using System.Text.Json;
using Quarter.Core.Utils;
using NUnit.Framework;

namespace Quarter.Core.UnitTest.Utils
{
    [TestFixture]
    public class UtcDateTimeTest
    {
        [Test]
        public void ItShouldCreateUsingUtc()
        {
            var utc = UtcDateTime.Now();

            Assert.That(utc.DateTime.Kind, Is.EqualTo(DateTimeKind.Utc));
        }

        [Test]
        public void ItShouldConvertDateTimeToUtc()
        {
            var nowInTest = DateTime.Now;

            Assert.That(new UtcDateTime(nowInTest).DateTime, Is.EqualTo(nowInTest.ToUniversalTime()));
        }

        [Test]
        public void ItShouldBeDeserializable()
        {
            var json = @"{ ""DateTime"": ""2021-07-15T20:21:19.678629Z"" }";
            var result = JsonSerializer.Deserialize<UtcDateTime>(json);

            // Just assert one thing, the problem was that date was DateTime.MinValue
            Assert.That(result.DateTime.Day, Is.EqualTo(15));
        }

        public void ItShouldOutputIso8601FormattedString()
        {
            var udt = UtcDateTime.FromUtcDateTime(DateTime.Parse("2021-07-15T20:21:19.678629Z"));

            Assert.That(udt.IsoString(), Is.EqualTo("2021-07-15T20:21:19.678629Z"));
        }


        [Test]
        public void ItShouldHaveAMinValue()
            => Assert.That(UtcDateTime.MinValue.DateTime, Is.EqualTo(DateTime.MinValue));

        [Test]
        public void ItShouldBeEqual()
        {
            var nowInTest = DateTime.Now;
            var one = new UtcDateTime(nowInTest);
            var two = new UtcDateTime(nowInTest);

            Assert.Multiple(() =>
            {
                Assert.That(one == two, Is.True);
                Assert.That(one.Equals(two), Is.True);
            });
        }

        [Test]
        public void ItShouldNotBeEqual()
        {
            var one = new UtcDateTime(DateTime.Now);
            var two = new UtcDateTime(DateTime.Now);

            Assert.Multiple(() =>
            {
                Assert.That(one != two, Is.True);
                Assert.That(one.Equals(two), Is.False);
            });
        }
    }
}