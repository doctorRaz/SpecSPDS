//https://replit.com/@razygraev/Log-Service-interface
using System;

namespace drz.Abstractions.Logger
{
    public interface IDrzLogService
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

        /// <summary>Gets the logger.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="productName">Name of the product.</param>
        /// <returns></returns>
        IDrzLogger GetLogger<T>(string productName);
       

        #endregion Public Methods
    }
}
