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
    public static class LoggerProvider//x прибить
    {
        #region Private Fields

        private static readonly Lazy<IDrzLoggerFactory> _service = new(() =>
            new NLogLoggerFactory(
                productNameProvider: () => AddonInfo.ProductName,
                assemblyDirectoryProvider: () => AddonInfo.AssemblyDirectory,
                envInfoProvider: new CadEnvironmentInfoProvider()
            ));

        #endregion Private Fields

        #region Internal Methods

        internal static IDrzLogger For<T>() => _service.Value.GetLogger<T>();

        internal static IDrzLogger For(Type type) => _service.Value.GetLogger(type);

        internal static IDrzLogger For<T>(string productName) => _service.Value.GetLogger<T>(productName);


        #endregion Internal Methods
    }


}