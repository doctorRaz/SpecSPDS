using NLog;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace dRz.SpecSpds.Test._experimental
{
    /// <summary>
    /// тест логера
    /// </summary>
    internal class TestLogCuctom
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        internal void Test()
        {
            Stopwatch stw = new Stopwatch();
            int count = 0;


            var assembly = Assembly.GetExecutingAssembly();

            Version version = assembly.GetName().Version; ;

            string ver = $"{version.Major}.{version.Minor}.{version.Build}";

            string name = assembly.GetName().Name;





            count++;

            stw.Restart();
            for (int i = 1; i <= 10; ++i)
            {

                log.Trace($" - {dat} {i} тест {count}");
            }

            stw.Stop();
            log.Trace($"********THE END************");
            log.Trace($"Total Time {stw.Elapsed.ToString()}");

            Test.WriteLine($"*********** THE END {stw.Elapsed.ToString()} *****************");
        }
        static string dat => DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.FFFFF", CultureInfo.InvariantCulture);
    }

}
