using System.Diagnostics;

namespace dRz.SpecSpds.Test.nLogTest
{
    internal class NlogDebug
    {

        internal void Test()
        {

            Stopwatch stopwatch = Stopwatch.StartNew();

            //write test nlog
            LogTestWrite logDebug = new LogTestWrite();



            for (int i = 0; i <  1000000; i++)
            {
                //GlobalDiagnosticsContext.Set("Caller", nameof(Test));//todo на скаку путь имя лога переключать нельзя!!!!

                logDebug.Test();
            }

            //stopwatch.Stop();
            //Console.WriteLine($"Total time: {stopwatch.Elapsed}");

        }
    }
}
