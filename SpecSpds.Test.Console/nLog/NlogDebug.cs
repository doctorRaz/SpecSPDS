using dRz.Loader.Cad.Bootstrap;
using dRz.Loader.Cad.InternalDiagnostic;
using NLog;

namespace dRz.SpecSpds.Test.nLog
{
    internal class NlogDebug
    {

        internal void Test()
        {
            //писатель в debug internal nlog
            InternalLoggerDiagnostic.Writer();

            GlobalDiagnosticsContext.Set("","");

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

            //write test nlog
            LogTestWrite logDebug = new LogTestWrite();

                      

            for (int i = 0; i < 1000; i++)
            {
                logDebug.Test();
           
            }

            //shutdown nlog
            LogManager.Shutdown();

            //Console.ReadKey();
        }
    }
}
