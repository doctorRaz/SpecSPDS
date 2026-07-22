//https://replit.com/@razygraev/Log-Service-interface
using System;

namespace drz.Abstractions.Logger
{
    /// <summary>
    /// Фабрика логеров. (Logger factory)
    /// </summary>
    public interface IDrzLoggerFactory
    {
        #region Public Methods

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IDrzLogger GetLogger<T>();

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        IDrzLogger GetLogger(Type type);

        #endregion Public Methods
    }
}