using dRz.Loader.Cad.Bootstrap;
using dRz.Loader.Cad.Infrastructure;
using dRz.Loader.Cad.InternalDiagnostic;
using dRz.SpecSPDS.Core._experimental;
using NLog;
using System;
using System.Diagnostics;

namespace dRz.SpecSpds.Test.nLog
{
    internal class NlogDebug
    {

        internal void Test()
        {
            
            Stopwatch stopwatch = Stopwatch.StartNew();

            //писатель в debug internal nlog
            //InternalLoggerDiagnostic.Writer();

            GlobalDiagnosticsContext.Set("LogsDir",LoaderEnvironment.AppDataProductLogPath);
            GlobalDiagnosticsContext.Set("AppName",LoaderEnvironment.ProductName);



            var conf = LogManager.Configuration;

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

            conf = LogManager.Configuration;
                      
            //write test nlog
            LogTestWrite logDebug = new LogTestWrite();

            LogDebugCore loggebCore = new LogDebugCore();      

            for (int i = 0; i < 100000; i++)
            {
                logDebug.Test();
                loggebCore.Test();
           
            }

            //shutdown nlog
            LogManager.Shutdown();

            stopwatch.Stop();
            Console.WriteLine($"Total time: {stopwatch.Elapsed}");
            Console.ReadKey();
        }
    }
}
