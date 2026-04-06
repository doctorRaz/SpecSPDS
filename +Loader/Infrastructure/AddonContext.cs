using dRz.Cad.Diagnostics.AddOn;

namespace dRz.Loader.Infrastructure
{
    internal static class AddonContext
    {
        public static readonly InfoAddOn InfoDll = InfoAddOn.Get(typeof(AddonContext));
        
    }
}
