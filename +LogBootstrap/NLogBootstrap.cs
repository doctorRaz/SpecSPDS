using dRz.Abstractions.Infrastructure;
using dRz.Abstractions.Logger;
using dRz.LogBootstrap.Builder;
using dRz.LogBootstrap.drzNLog;
using NLog;
using System.Collections.Concurrent;

namespace dRz.LogBootstrap
{
    /// <summary>
    /// найти или создать IDrzLoggerFactory
    /// </summary>
    public class NLogBootstrap
    {
        private static readonly ConcurrentDictionary<string, IDrzLoggerFactory> _factories = new();

        /// <summary>Gets the logger factory.</summary>
        /// <param name="addOnInfo">The add on information.</param>
        /// <returns></returns>
        public static IDrzLoggerFactory GetLoggerFactory(IAddOnInfo addOnInfo)
        {
            return _factories.GetOrAdd(
                addOnInfo.Product, _ =>
                {
                    NLogFactoryBuilder builder = new(addOnInfo);
                    LogFactory logFactory = builder.Build();
                    return new NLogLoggerFactory(logFactory);
                });
        }
    }
}