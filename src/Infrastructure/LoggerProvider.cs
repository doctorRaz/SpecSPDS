using drz.Abstractions.Logger;
using drz.EnvironmentInfo;
using drz.LogServices;
using drz.LogServices.Interfaces;
using NLog;
using System;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.Src.Infrastructure
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

        internal static Logger For<T>() => _service.Value.GetLogger<T>();

        internal static Logger For(Type type) => _service.Value.GetLogger(type);
    }

    /// <summary>
    /// Проброс в фабрику инфы о ОС КАД и аддоне
    /// </summary>
    /// <seealso cref="Abstractions.Logger.IEnvironmentInfoProvider" />
    public class CadEnvironmentInfoProvider : IEnvironmentInfoProvider
    {
        /// <summary>
        /// Gets the summary.
        /// </summary>
        /// <returns></returns>
        public string GetSummary()
        {
            //порнография конечно( /InfoDll/
            return $"{RT.Info.ToString()}\n{InfoDll.ToString()}";
        }
    }
}