using dRz.Cad.Diagnostics.AddOn;
using dRz.Loader.Infrastructure;
using NLog;
using static dRz.Loader.Infrastructure.AddonContext;

namespace dRz.SpecSPDS
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


        public void LogTest(string msg)
        {

            log.Info(msg);

        }
    }
}