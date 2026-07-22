using drz.Lib_A;
using drz.Lib_B;
using System;
using System.Diagnostics;

namespace drz.SpecSpds.Test.SimpleInjector
{
    /// <summary>
    /// SimpleInjector TestUpdater
    /// </summary>
    internal class DrzSimpleInjector
    {
        #region Internal Methods

        internal void Start()
        {
             Stopwatch sw = Stopwatch.StartNew();
           

            TestContainer testContainer = new TestContainer();
            Console.WriteLine($"{sw.Elapsed} new TestContainer();");
            sw.Restart();

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