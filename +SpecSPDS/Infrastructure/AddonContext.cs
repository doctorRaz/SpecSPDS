using drz.Cad.Diagnostics.AddOn;

namespace drz.SpecSPDS.Infrastructure
{
    internal static class AddonContext
    {
        public static readonly InfoAddOn InfoDll = InfoAddOn.Get(typeof(AddonContext));
    }
}