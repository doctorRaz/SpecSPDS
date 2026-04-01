using dRz.Cad.Diagnostics.AddOn;
using dRz.SpecSpds.Test.Loader;
using NLog;
using static dRz.Loader.Infrastructure.AddonContext;

namespace AddonA
{
    public class CommandA
    {
         ILogger  log = LoggerProvider.For<CommandA>();
        public InfoAddOn Execute()
        {
            return InfoDll;

            //Console.WriteLine(AddonContext.InfoDll);

            //Console.WriteLine($"AddonA: {RT.Info}");



        }


        public void LogTest()
        {

            log.Info("CommandA");

        }
    }
}