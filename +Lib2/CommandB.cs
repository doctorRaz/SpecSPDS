using dRz.Cad.Diagnostics.AddOn;
using static dRz.Loader.Infrastructure.AddonContext;

namespace AddonB
{
    public class CommandB
    {
      
        public InfoAddOn Execute()
        {
            return InfoDll;
            //Console.WriteLine(AddonContext.InfoDll);

            //Console.WriteLine($"AddonB: {RT.Info}");
        }
    }
}