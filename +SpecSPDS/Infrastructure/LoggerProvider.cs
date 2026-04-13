using drz.EnvironmentInfo;
using drz.LogServices;
using drz.LogServices.Interfaces;
using NLog;
using System;
using static drz.SpecSPDS.Infrastructure.AddonContext;

namespace drz.SpecSPDS.Infrastructure
{
    /// <summary>
    /// проброс в фабрику ProductName и получение логера для продукта
    /// </summary>
    public static class LoggerProvider
    {
        private static readonly Lazy<ILogService> _service = new(() =>
            new LogService(
                productNameProvider: () => InfoDll.ProductName,
                assemblyDirectoryProvider: () => InfoDll.AssemblyDirectory,
                envInfoProvider: new CadEnvironmentInfoProvider()
            ));

        public static Logger For<T>() => _service.Value.GetLogger<T>();

        public static Logger For(Type type) => _service.Value.GetLogger(type);
    }

    /// <summary>
    /// Проброс в фабрику инфы о ОС КАД и аддоне
    /// </summary>
    /// <seealso cref="drz.LogServices.Interfaces.IEnvironmentInfoProvider" />
    public class CadEnvironmentInfoProvider : IEnvironmentInfoProvider
    {
        public string GetSummary()
        {
            //порнография конечно(
            return $"{RT.Info.ToString()}\n{InfoDll.ToString()}";
        }
    }
}