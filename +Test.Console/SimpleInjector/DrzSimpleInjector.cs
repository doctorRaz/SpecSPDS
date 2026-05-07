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

            testContainer.TestInjectorInfo();
            
            testContainer.TestInjectorMessage();

            CommandA cmdA = new CommandA();
            cmdA.msgCommandA();

            CommandB cmdB = new CommandB();
            cmdB.msgCommandB();

            testContainer.TestInjectorMessage();
            cmdA.msgCommandA();
            cmdB.msgCommandB();
        }

        #endregion Internal Methods
    }
}