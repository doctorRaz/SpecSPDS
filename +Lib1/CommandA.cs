using dRz.Cad.Diagnostics.AddOn;
using static dRz.Loader.Infrastructure.AddonContext;

namespace AddonA
{
    public class CommandA
    {
        public InfoAddOn Execute()
        {
            return InfoDll;

            //Console.WriteLine(AddonContext.InfoDll);

            //Console.WriteLine($"AddonA: {RT.Info}");



        }
    }
}