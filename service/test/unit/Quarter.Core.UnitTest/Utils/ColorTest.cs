using System.Collections.Generic;
using NUnit.Framework;
using Quarter.Core.Utils;

namespace Quarter.Core.UnitTest.Utils
{
    [TestFixture]
    public class ColorTest
    {
        [Test]
        public void AreEqual()
        {
            var one = new Color(System.Drawing.Color.Blue.ToArgb());
            var two = new Color(System.Drawing.Color.Blue.ToArgb());

            Assert.That(one, Is.EqualTo(two));
        }

        [Test]
        public void AreNotEqual()
        {
            var one = new Color(System.Drawing.Color.Blue.ToArgb());
            var two = new Color(System.Drawing.Color.Yellow.ToArgb());

            Assert.That(one, Is.Not.EqualTo(two));
        }

        public static IEnumerable<object[]> HexTests()
        {
            yield return ["#ffffff", System.Drawing.Color.White];
            yield return ["#000000", System.Drawing.Color.Black];
            yield return ["#00ffff", System.Drawing.Color.Cyan];
            yield return ["#fff", System.Drawing.Color.White];
            yield return ["#000", System.Drawing.Color.Black];
        }

        [TestCaseSource(nameof(HexTests))]
        public void CanBeConstructedFromHexString(string hex, System.Drawing.Color systemColor)
        {
            var color = Color.FromHexString(hex);
            var expectedColor = Color.FromSystemColor(systemColor);
            Assert.That(color, Is.EqualTo(expectedColor));
        }

        public static IEnumerable<object[]> ToHexTests()
        {
            yield return [System.Drawing.Color.White, "#FFFFFF"];
            yield return [System.Drawing.Color.Black, "#000000"];
            yield return [System.Drawing.Color.Cyan, "#00FFFF"];
        }

        [TestCaseSource(nameof(ToHexTests))]
        public void CanConvertToHexString(System.Drawing.Color systemColor, string expectedHex)
        {
            var color = Color.FromSystemColor(systemColor);
            Assert.That(color.ToHex(), Is.EqualTo(expectedHex));
        }

        public static IEnumerable<object[]> HexDarkerTests()
        {
            yield return ["#ffffff", "#E5E5E5"];
            yield return ["#000000", "#000000"];
            yield return ["#00ffff", "#00E5E5"];
            yield return ["#a58fe5", "#9480CE"];
        }

        [TestCaseSource(nameof(HexDarkerTests))]
        public void CanConstructDarkerColor(string hex, string darkerHex)
        {
            var color = Color.FromHexString(hex);
            var darkerColor = color.Darken(0.10);
            var expectedColor = Color.FromHexString(darkerHex);

            Assert.That(darkerColor, Is.EqualTo(expectedColor));
        }
    }
}
