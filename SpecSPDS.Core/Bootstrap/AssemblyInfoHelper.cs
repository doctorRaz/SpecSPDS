using System.Reflection;

namespace dRz.SpecSPDS.Core.Bootstrap
{
    //https://github.com/doctorRaz/SpecSPDS/wiki/Bootstrap#реализация-рекомендуемая
    internal static class AssemblyInfoHelper
    {
        internal static string GetProductFromAssembly(Assembly assembly)
        {
            return assembly
                .GetCustomAttribute<AssemblyProductAttribute>()?.Product
                ?? assembly.GetName().Name!;
        }
    }
}
