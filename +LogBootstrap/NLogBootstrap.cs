using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.LogBootstrap.Builder;
using drz.LogBootstrap.drzNLog;
using NLog;
using System.Collections.Concurrent;

namespace drz.LogBootstrap
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
                addOnInfo.ProductName, _ =>
                {
                    NLogFactoryBuilder builder = new(addOnInfo);
                    LogFactory logFactory = builder.Build();
                    return new NLogLoggerFactory(logFactory);
                });
        }
    }
}