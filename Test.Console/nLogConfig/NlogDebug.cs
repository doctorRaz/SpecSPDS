using dRz.Loader.Cad.Infrastructure.Bootstrap;
using dRz.Loader.Cad.Infrastructure.InternalDiagnostic;
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

            //GlobalDiagnosticsContext.Set("LogsDir",LoaderEnvironment.AppDataProductLogPath);
            //GlobalDiagnosticsContext.Set("AppName",LoaderEnvironment.ProductName);



            //var conf = LogManager.Configuration;

            //если лог конфиг не загрузился сам грузим руками
            //if (LogManager.Configuration is null)
            //{
            //пытаемся грузить принудительно
             //LogBootstrap/*.Init*/();
             _LogBootstrap.Initialize();


            //}

            var conf = LogManager.Configuration;

            //write test nlog
            LogTestWrite logDebug = new LogTestWrite();

            LogDebugCore loggebCore = new LogDebugCore();

            for (int i = 0; i < 1; i++)
            {
                GlobalDiagnosticsContext.Set("Caller", nameof(Test));//todo на скаку путь имя лога переключать нельзя!!!!

                logDebug.Test();

                loggebCore.Test();

            }

            //shutdown nlog
            LogManager.Shutdown();

            stopwatch.Stop();
            Console.WriteLine($"Total time: {stopwatch.Elapsed}");
            //  Console.ReadKey();
        }
    }
}
