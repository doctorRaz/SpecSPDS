using dRz.Loader.Infrastructure;
using NLog;

namespace dRz.SpecSPDS
{
    public class CommandA
    {
        private ILogger log = LoggerProvider.For<CommandA>();
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