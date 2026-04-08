using drz.Loader.Infrastructure;
using NLog;

namespace drz.SpecSpds.Test.Tests
{
    internal class LogTests
    {
        private ILogger log = LoggerProvider.For<LogTests>();

        internal void LogTest(string msg)
        {
            log.Info(msg);

        }

        internal string Execute()
        {
            CadEnvironmentInfoProvider ff = new CadEnvironmentInfoProvider();

            return ff.GetSummary();
        }

    }
}
