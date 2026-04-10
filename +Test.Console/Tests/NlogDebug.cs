using System.Diagnostics;

namespace drz.SpecSpds.Test.Tests
{
    internal class NlogDebug
    {

        internal void Test()
        {

            Stopwatch stopwatch = Stopwatch.StartNew();

            //write test nlog
            LogTestWrite logDebug = new LogTestWrite();



            for (int i = 0; i < 1  /*1000000*/; i++)
            {
                //GlobalDiagnosticsContext.Set("Caller", nameof(Test));//todo на скаку путь имя лога переключать нельзя!!!!

                logDebug.Test();
            }

            //stopwatch.Stop();
            //ConsoleMessage.WriteLine($"Total time: {stopwatch.Elapsed}");

        }
    }
}
