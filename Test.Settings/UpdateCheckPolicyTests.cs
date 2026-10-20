using dRz.Abstractions.Settings;
using NUnit.Framework;

namespace dRz.Test.Settings
{
    [TestFixture]
    public sealed class UpdateCheckPolicyTests
    {
        [TestCase(-100, 1)]
        [TestCase(0, 1)]
        [TestCase(1, 1)]
        [TestCase(7, 7)]
        [TestCase(30, 30)]
        [TestCase(31, 30)]
        [TestCase(10000000, 30)]
        public void NormalizeIntervalDays_ClampsToOneThroughThirty(int configured, int expected)
        {
            Assert.That(UpdateCheckPolicy.NormalizeIntervalDays(configured), Is.EqualTo(expected));
        }
    }
}