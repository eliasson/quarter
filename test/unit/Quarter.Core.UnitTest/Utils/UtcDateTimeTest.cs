using System;
using System.Text.Json;
using Quarter.Core.Utils;
using NUnit.Framework;

namespace Quarter.Core.UnitTest.Utils
{
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
        public void ItShouldBeEqual()
        {
            var nowInTest = DateTime.Now;

            Assert.That(new UtcDateTime(nowInTest), Is.EqualTo(new UtcDateTime(nowInTest)));
        }

        [Test]
        public void ItShouldNotBeEqual()
        {
            var now = new UtcDateTime(DateTime.Now);
            var future = new UtcDateTime(DateTime.Now.AddMilliseconds(1));

            Assert.That(now, Is.Not.EqualTo(future));
        }

        [Test]
        public void ItShouldBeDeserializable()
        {
            var json = @"{ ""DateTime"": ""2021-07-15T20:21:19.678629Z"" }";
            var result = JsonSerializer.Deserialize<UtcDateTime>(json);

            // Just assert one thing, the problem was that date was DateTime.MinValue
            Assert.That(result.DateTime.Day, Is.EqualTo(15));
        }
    }
}