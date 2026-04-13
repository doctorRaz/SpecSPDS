using drz.Lib_A;
using drz.Lib_B;

namespace drz.SpecSpds.Test.SimpleInjector
{
    /// <summary>
    /// SimpleInjector Test
    /// </summary>
    internal class DrzSimpleInjector
    {
        #region Internal Methods

        internal void Start()
        {
            TestContainer testContainer = new TestContainer();
            testContainer.TestCondole();

            CommandA cmdA = new CommandA();
            cmdA.msgCommandA();

            CommandB cmdB = new CommandB();
            cmdB.msgCommandB();

            testContainer.TestCondole();
            cmdA.msgCommandA();
            cmdB.msgCommandB();
        }

        #endregion Internal Methods
    }
}