using dRz.SpecSPDS.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace dRz.SpecSpdsConsole
{
    /// <summary>
    /// тест логера
    /// </summary>
    internal class TestLogCuctom
    {

        internal void Test()
        {
            Stopwatch stw = new Stopwatch();
            int count = 0;


            var assembly = Assembly.GetExecutingAssembly();

            Version version = assembly.GetName().Version; ;

            string ver = $"{version.Major}.{version.Minor}.{version.Build}";

            string name = assembly.GetName().Name;

            Logger log = new Logger($"{name}_v {ver}");

            log.LogAllClear();

            count++;

            stw.Restart();
            for (int i = 1; i <= 10; ++i)
            {

                log.Log($" - {dat} {i} тест {count}");
            }

            stw.Stop();
            log.Log($"********THE END************");
            log.Log($"Total Time {stw.Elapsed.ToString()}");

            Console.WriteLine($"*********** THE END {stw.Elapsed.ToString()} *****************");
        }
        static string dat => DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.FFFFF", CultureInfo.InvariantCulture);
    }

}
