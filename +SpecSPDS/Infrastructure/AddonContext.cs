using drz.EnvironmentInfo.App;

namespace drz.SpecSPDS.Infrastructure
{
    internal static class AddonContext
    {
        public static readonly AppInfo InfoDll = AppInfo.Get(typeof(AddonContext));
    }
}