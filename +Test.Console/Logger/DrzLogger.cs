using drz.Lib_A;
using drz.Lib_B;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drz.SpecSpds.Test.Logger
{
    /// <summary>
    /// Logger Test
    /// </summary>
    internal class DrzLogger
    {
        internal void Start()
        {
            CommandA cmdA = new CommandA();

            CommandB cmdB = new CommandB();

            LogTestWrite logTestWrite = new LogTestWrite();

            logTestWrite.Test();

            LogTests logTests = new LogTests();

            cmdA.LogTest("A0ttrrt");

            cmdB.LogTest("B0trte");

            logTests.LogTest("Test0tet");

            cmdA.LogTest("2");

            cmdB.LogTest("2");

            cmdA.LogTest("20");

            cmdB.LogTest("20");
        }
    }
}