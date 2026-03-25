using dRz.CAD.Runtime.Info;

namespace dRz.Loader.Infrastructure
{
    internal static class AddonContext
    {
        public static readonly InfoAdOn InfoDll = InfoAdOn.Get(typeof(AddonContext));
    }
}
