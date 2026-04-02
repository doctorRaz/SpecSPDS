using dRz.Cad.Diagnostics.AddOn;
using dRz.Loader.Infrastructure;
using NLog;
using static dRz.Loader.Infrastructure.AddonContext;

namespace dRz.SpecSPDS
{
    public class CommandB
    {
        ILogger  log = LoggerProvider.For<CommandB>();
      
        public InfoAddOn Execute()
        {
            return InfoDll;
            //Console.WriteLine(AddonContext.InfoDll);

            //Console.WriteLine($"AddonB: {RT.Info}");
        }

        public void LogTest(string msg)
        {

            log.Info(msg);

        }
    }
}