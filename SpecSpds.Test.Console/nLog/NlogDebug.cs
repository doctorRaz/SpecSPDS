using NLog;
using System;
using dRz.SpecSPDS.Core.InternalDiagnostic;
using dRz.Experimental.Bootstrap;
using dRz.SpecSPDS.Core._experimental;
using NLog.Common;

namespace dRz.SpecSpds.Test.nLog
{
    internal class NlogDebug
    {

        internal void Test()
        {
            //писатель в debug internal nlog
            InternalLoggerDiagnostic.Writer();

            //если лог конфиг не загрузился сам грузим руками
            if (LogManager.Configuration is null)
            {
                //пытаемся грузить принудительно
                new LogBootstrap();

                //если конфиг не нашелся и не загрузился
                if (LogManager.Configuration is null)
                {
                    //включим диагностику eсли выключена
                    new InternalLoggerDiagnostic("LogManager empty Configuration");

                    //дальше пишем внутренний лог
                }
            }
            
            //write test nlog
            LogTestWrite logDebug = new LogTestWrite();

            logDebug.Test();


            //write   loader nlog
            LogDebugCore logDebugCore = new LogDebugCore();

            logDebugCore.Test();

            //shutdown nlog
            LogManager.Shutdown();

            //Console.ReadKey();
        }
    }
}
