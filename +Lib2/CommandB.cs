using dRz.Cad.Diagnostics.AddOn;
using dRz.SpecSpds.Test.Loader;
using NLog;
using static dRz.Loader.Infrastructure.AddonContext;

namespace AddonB
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

        public void LogTest()
        {

            log.Info("CommandB");

        }
    }
}