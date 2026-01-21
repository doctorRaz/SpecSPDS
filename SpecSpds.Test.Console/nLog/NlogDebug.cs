using NLog;
using System;
using dRz.SpecSPDS.Core.InternalDiagnostic;
using dRz.Experimental.Bootstrap;
using dRz.SpecSPDS.Core._experimental;

namespace dRz.SpecSpds.Test.nLog
{
    internal class NlogDebug
    {

        internal void Test()
        {

            //debug internal nlog
            InternalLoggerDiagnostic.InternalLoggerInit();

            //init nlog
            LogBootstrapper.Init();

            //write test nlog
            LogTestWrite logDebug = new LogTestWrite();

            logDebug.Test();

            //write   core nlog
            LogDebugCore logDebugCore = new LogDebugCore();

            logDebugCore.Test();

            //shutdown nlog
            LogManager.Shutdown();

            Console.ReadKey();
        }
    }
}
