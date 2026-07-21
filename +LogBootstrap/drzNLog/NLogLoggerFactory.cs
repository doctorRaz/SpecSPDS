using drz.Abstractions.Logger;
using NLog;
using System;

//https://replit.com/@razygraev/Log-Service-interface

namespace drz.LogBootstrap.drzNLog
{
    /// <summary>
    /// Log Factory
    /// </summary>
    /// <seealso cref="IDrzLoggerFactory" />
    public sealed class NLogLoggerFactory : IDrzLoggerFactory
    {
        private readonly LogFactory _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLoggerFactory"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public NLogLoggerFactory(LogFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IDrzLogger GetLogger<T>() => GetLogger(typeof(T));

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">type</exception>
        public IDrzLogger GetLogger(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return new NLogAdapter(_factory.GetLogger(type.FullName));
        }
    }
}