using NLog;
using System;

namespace dRz.SpecSpds.Test.nLog
{
    internal class LogTestWrite
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        internal void Test()
        {
            int b = 0;

            int calcs()
            {
                int a = 10;

                return a + b;
            }
            b = calcs();

            if (log.IsDebugEnabled)
                log.Warn("This is a message from {Calc}", calcs());

            int i0 = 0;
            try
            {

                if (log.IsDebugEnabled)
                    log.ForTraceEvent()
                       .Message("Начало работы")
                       .Property("prop1", calcs())
                       .Property("prop2", 123)
                       .Log();

                int e = 0;

                int ii = 10 / e;
            }
            catch (Exception ex)
            {
                log.ForErrorEvent()
                   .Exception(ex)
                   .Property("prop1", 50000)
                   .Property("prop2", 123)
                   .Log();

                log.Info("Продолжение работы после ошибки");

                log.Error(ex);

            }
            finally
            {

            }

        }
    }
}
