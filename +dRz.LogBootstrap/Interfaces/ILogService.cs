using NLog;
using System;

namespace drz.LogServices.Interfaces
{
    /// <summary>
    ///
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Logger GetLogger<T>();

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        Logger GetLogger(Type type);
    }
}