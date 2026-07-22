using drz.Lib_A;
using drz.Lib_B;

namespace drz.SpecSpds.Test.Logger
{
    /// <summary>
    /// Logger TestUpdater
    /// </summary>
    internal class DrzLogger
    {
        internal void Start()
        {
            LogFluentTest logFluentTest = new LogFluentTest();

            logFluentTest.Test();

            CommandA cmdA = new CommandA();

            CommandB cmdB = new CommandB();

            LogTestWrite logTestWrite = new LogTestWrite();

            logTestWrite.Test();

            cmdA.LogTest("cmdA");

            cmdB.LogTest("cmdB");


        }
    }
}