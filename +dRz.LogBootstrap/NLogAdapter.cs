//https://replit.com/@razygraev/Log-Service-interface

using NLog;
using System;
using drz.LogServices.Interfaces;
using drz.Abstractions.Logger;
using drz.LogServices;

namespace drz.LogServices
{
    internal sealed class NLogAdapter : IDrzLogger
    {
        #region Private Fields

        private readonly Logger _inner;

        #endregion Private Fields

        #region Internal Constructors

        internal NLogAdapter(Logger inner)
        {
            _inner = inner;
        }

        #endregion Internal Constructors

        #region Public Properties

        public bool IsDebugEnabled => _inner.IsDebugEnabled;
        public bool IsErrorEnabled => _inner.IsErrorEnabled;
        public bool IsFatalEnabled => _inner.IsFatalEnabled;
        public bool IsInfoEnabled => _inner.IsInfoEnabled;
        public bool IsTraceEnabled => _inner.IsTraceEnabled;
        public bool IsWarnEnabled => _inner.IsWarnEnabled;

        #endregion Public Properties

        #region Public Methods

        public void Debug(string message) => _inner.Debug(message);

        public void Error(string message, Exception exception = null) => _inner.Error(exception, message);

        public void Fatal(string message, Exception exception = null) => _inner.Fatal(exception, message);

        public void Info(string message) => _inner.Info(message);

        public void Trace(string message) => _inner.Trace(message);
        public void Warn(string message) => _inner.Warn(message);

        #endregion Public Methods
    }
}

public class NLogService : IDrzLogService
{
    #region Public Methods

    public IDrzLogger GetLogger<T>() => GetLogger(typeof(T));

    public IDrzLogger GetLogger(Type type)
    {
        //if (type == null) throw new ArgumentNullException(nameof(type));
        //string productName = SafeGetProductName();
        //LogFactory factory = _factories.GetOrAdd(productName, CreateFactory);
        //return new NLogAdapter(factory.GetLogger(type.FullName)); // <-- оборачиваем
        throw new NotSupportedException ("Не доделано");
    }

    #endregion Public Methods
}