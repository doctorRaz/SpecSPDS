using dRz.Cad.Diagnostics.AddOn;
using dRz.Loader.Infrastructure;
using NLog;
using static dRz.Loader.Infrastructure.AddonContext;

namespace dRz.SpecSPDS
{
    public class CommandA
    {
         ILogger  log = LoggerProvider.For<CommandA>();
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