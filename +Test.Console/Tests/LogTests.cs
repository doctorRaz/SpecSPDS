using dRz.Loader.Infrastructure;
using NLog;

namespace dRz.SpecSpds.Test.Tests
{
    internal class LogTests
    {
          ILogger  log = LoggerProvider.For<LogTests>();

        internal void LogTest(string msg)
        {
            log.Info(msg);

        }

    }
}
