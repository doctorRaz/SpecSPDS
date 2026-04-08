using drz.Cad.Diagnostics.AddOn;

namespace drz.Loader.Infrastructure
{
    internal static class AddonContext
    {
        public static readonly InfoAddOn InfoDll = InfoAddOn.Get(typeof(AddonContext));

    }
}
