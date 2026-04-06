using dRz.Cad.Diagnostics;
using dRz.LogServices;
using dRz.LogServices.Interfaces;
using NLog;
using System;
using static dRz.Loader.Infrastructure.AddonContext;

namespace dRz.Loader.Infrastructure
{
    public static class LoggerProvider
    {
        private static readonly Lazy<ILogService> _service = new(() =>
            new LogService(
                productNameProvider: () => InfoDll.ProductName,
                assemblyDirectoryProvider: () => null, /*InfoDll.AssemblyDirectory,*/
                envInfoProvider: new CadEnvironmentInfoProvider()
            ));

        public static Logger For<T>() => _service.Value.GetLogger<T>();

        public static Logger For(Type type) => _service.Value.GetLogger(type);
    }



    public class CadEnvironmentInfoProvider : IEnvironmentInfoProvider
    {
        public string GetSummary()
        {
            //порнография конечно(
            return $"{RT.Info.ToString()}\n{InfoDll.ToString()}";
        }
    }
}