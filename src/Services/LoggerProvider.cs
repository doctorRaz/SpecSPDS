using drz.Abstractions.Logger;
using drz.LogServices.drzNlog;
using drz.Src.Infrastructure;
using System;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.Src.Services
{
    /// <summary>
    /// проброс в фабрику ProductName и получение логера для продукта
    /// </summary>
    public static class LoggerProvider
    {
        #region Private Fields

        private static readonly Lazy<IDrzLogService> _service = new(() =>
            new NLogService(
                productNameProvider: () => InfoDll.ProductName,
                assemblyDirectoryProvider: () => InfoDll.AssemblyDirectory,
                envInfoProvider: new CadEnvironmentInfoProvider()
            ));

        #endregion Private Fields

        #region Internal Methods

        internal static IDrzLogger For<T>() => _service.Value.GetLogger<T>();

        internal static IDrzLogger For(Type type) => _service.Value.GetLogger(type);

        #endregion Internal Methods
    }


}