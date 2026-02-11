using System;
using NUnit.Framework;
using System.Collections.Generic;

namespace Checks
{
    public class BatchConverterTest
    {
        [Test]
        [TestCaseSource(nameof(GetToIntCases))]
        public void ToInt(string value, object expected)
        {
            Converter.Converter converter = new Converter.Converter();
            if (expected is Exception expectedException)
            {
                Assert.Throws(expectedException.GetType(), () => converter.ToInt(value));
            }
            else
            {
                Assert.That(converter.ToInt(value), Is.EqualTo(expected));
            }
        }

        private static IEnumerable<TestCaseData> GetToIntCases()
        {
            yield return new TestCaseData("123", 123);
            yield return new TestCaseData("0", 0);
            yield return new TestCaseData("abc", new ArgumentException());
            yield return new TestCaseData("-1", -1);
            yield return new TestCaseData("-1 1", new ArgumentException());
        }
    }
}
