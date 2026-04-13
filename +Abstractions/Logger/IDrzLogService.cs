//https://replit.com/@razygraev/Log-Service-interface
using System;

namespace drz.Abstractions.Logger
{


    /// <summary>
    ///
    /// </summary>
    public interface IDrzLogger
    {
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
    }


    /// <summary>
    ///
    /// </summary>
    public interface IDrzLogService
    {
        void Debug(string message);
        void Error(string message, Exception exception = null);

        void Fatal(string message, Exception exception = null);

        void Info(string message);

        void Warn(string message);
    }
}