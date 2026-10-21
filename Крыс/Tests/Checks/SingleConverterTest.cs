using NUnit.Framework;
using System;

namespace Checks
{
    public class SingleConverterTest
    {
        [Test]
        public void ToInt()
        {
            Converter.Converter converter = new Converter.Converter();
            Assert.That(converter.ToInt("12"), Is.EqualTo(12));
            Assert.That(converter.ToInt("0"), Is.EqualTo(0));
            Assert.That(converter.ToInt("-1"), Is.EqualTo(-1));
            Assert.Throws<ArgumentException>(() => converter.ToInt("abc"));
        }
    }
}
