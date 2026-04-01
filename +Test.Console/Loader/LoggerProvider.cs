using dRz.Cad.Diagnostics;
using dRz.Log.Interfaces;
using NLog;
using System;
using static dRz.Loader.Infrastructure.AddonContext;

namespace dRz.SpecSpds.Test.Loader
{
    public static class LoggerProvider
    {
        private static readonly Lazy<ILogService> _service = new(() =>
            new LogService(
                productNameProvider: () => InfoDll.ProductName,
                configPathProvider: () => InfoDll.NLogConfigPath
            ));

        public static Logger For<T>() => _service.Value.GetLogger<T>();

        public static Logger For(Type type) => _service.Value.GetLogger(type);
    }
}