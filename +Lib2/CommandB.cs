using drz.Loader.Infrastructure;
using NLog;

namespace drz.SpecSPDS
{
    public class CommandB
    {
        private ILogger log = LoggerProvider.For<CommandB>();

        public string Execute()
        {
            CadEnvironmentInfoProvider ff = new CadEnvironmentInfoProvider();

            return ff.GetSummary();
        }

        public void LogTest(string msg)
        {

            log.Info(msg);

        }
    }
}