using NLog;
using System;
using System.Windows.Forms;

//todo удалить после отладки nlog

namespace dRz.SpecSPDS.Core._experimental
{
    public class LogDebugCore
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public  void Test()
        {
            int b=0;

            int calcs()
            {
                int a = 10;
              
                return a + b;
            }
            b=calcs();

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
