using dRz.Abstractions.Infrastructure;
using System.Reflection;

namespace dRz.Infrastructure.Infrastructure
{
    /// <summary>
    ////регистрация и получение IAddOnInfo
    /// </summary>
    public static class AddOnInfoProvider
    {

        public static IAddOnInfo GetOrAdd<T>() => GetOrAdd(typeof(T).Assembly);

        public static IAddOnInfo GetOrAdd(Type type) => GetOrAdd(type.Assembly);

        public static IAddOnInfo GetOrAdd(Assembly assembly)
        {
            //обращаемся к классу AddOnInfoRegistry напрямую, методы GetOrAdd будут удалены из IAddOnInfoRegistry
            AddOnInfoRegistry addOnInfoRegistry = new AddOnInfoRegistry();

            return addOnInfoRegistry.GetOrAdd(assembly);
        }

    }
}
