using NLog;
using System;

namespace dRz.SpecSpds.Test.nLogTest
{
    internal class LogTestWrite
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        internal void Test()
        {

            log.Trace("Сообщение {prop1} {prop2}", "1", "2");
            log.Debug("Сообщение {prop1} {prop2}", "1", "2");
            log.Info("Сообщение {prop1} {prop2}", "1", "2");
            log.Warn("Сообщение {prop1} {prop2}", "1", "2");
            log.Error("Сообщение {prop1} {prop2}", "1", "2");
            log.Fatal("Сообщение {prop1} {prop2}", "1", "2");

            int b = 0;

            int calcs()
            {
                int a = 10;

                return a + b;
            }
            b = calcs();

            if (log.IsDebugEnabled)
                log.Warn("This is a message from {Calc}", calcs());
            tt t = new tt();

            try
            {

                if (log.IsDebugEnabled)
                    log.ForTraceEvent()
                       .Message("Начало работы")
                       .Property("prop1", calcs())
                       .Property("prop2", 123)
                       .Log();

                log.ForTraceEvent()
                       .Message("Класс Продолжение работы")
                       .Property("prop10", calcs())
                          .Property("класс", t)
                       .Log();

                int e = 10;

                log.Trace("Сообщение {prop1} {prop2}", "1", "2");

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

        internal class tt
        {

            public string g = "10";
            public string g1 = "10";
            public string g2 = "10";
        }
    }
}
