using dRz.Cad.Diagnostics.AddOn;

namespace dRz.SpecSPDS.Infrastructure
{
    internal static class AddonContext
    {
        public static readonly InfoAdOn InfoDll = InfoAdOn.Get(typeof(AddonContext));
    }
}
