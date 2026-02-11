using dRz.Loader.Cad.Infrastructure;
using dRz.SpecSPDS.Core._experimental;
using dRz.SpecSPDS.Core.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dRz.SpecSpds.Test.nLog
{
    /// <summary>
    /// Тест скорости работы конфигов в зависимости как внутри них задаются пути и тд
    /// </summary>
    internal class TestSpeedConfig
    {
        string testPath = @"d:\@Developers\Programmers\!NET\!SpecSPDS\SpecSPDS\Shared\Test nLog config";
        internal void Run()
        {
            List<string> configs = FetchingPatchFiles.GetFilesOfDir(testPath, false, "*");

            GlobalDiagnosticsContext.Set("LogsDir", LoaderEnvironment.AppDataProductLogPath);
            GlobalDiagnosticsContext.Set("AppName", LoaderEnvironment.ProductName);

            Stopwatch sw = new Stopwatch();

            foreach (string config in configs)
            {
                string configName = Path.GetFileNameWithoutExtension(config);

                sw.Restart();

                LogManager.Setup().LoadConfigurationFromFile(config);

                if (LogManager.Configuration is null)
                {
                    Console.WriteLine($"{configName} FATAL");
                    continue;
                }

                //write test nlog
                LogTestWrite logDebug = new LogTestWrite();

                LogDebugCore loggebCore = new LogDebugCore();

                for (int i = 0; i < 1000000; i++)
                {
                    logDebug.Test();
                    loggebCore.Test();
                }

                sw.Stop();

                //shutdown nlog
                //LogManager.Flush(TimeSpan.FromSeconds(10));
                LogManager.Shutdown();



                Console.WriteLine($"{configName};{sw.ElapsedTicks};\t{sw.Elapsed}");
                //Console.WriteLine("Press eny key...");
                //Console.ReadKey();

            }

        }

    }
}
