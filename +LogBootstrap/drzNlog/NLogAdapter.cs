//https://replit.com/@razygraev/Log-Service-interface

using drz.Abstractions.Logger;
using NLog;
using System;

namespace drz.LogBootstrap.drzNlog
{
    internal sealed class NLogAdapter : IDrzLogger
    {
        #region Private Fields

        private readonly Logger _inner;

        #endregion Private Fields

        #region Internal Constructors

        internal NLogAdapter(Logger inner) => _inner = inner;

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

        public void Error(Exception exception, string message = null) => _inner.Error(exception, message);
        public void Fatal(Exception exception, string message = null) => _inner.Error(exception, message);

        public void Fatal(string message, Exception exception = null) => _inner.Fatal(exception, message);

        public ILogEventBuilder ForDebugEvent() => new NLogEventBuilderAdapter(_inner.ForDebugEvent());

        public ILogEventBuilder ForErrorEvent() => new NLogEventBuilderAdapter(_inner.ForErrorEvent());

        public ILogEventBuilder ForFatalEvent() => new NLogEventBuilderAdapter(_inner.ForFatalEvent());

        public ILogEventBuilder ForInfoEvent() => new NLogEventBuilderAdapter(_inner.ForInfoEvent());

        public ILogEventBuilder ForTraceEvent() => new NLogEventBuilderAdapter(_inner.ForTraceEvent());

        public ILogEventBuilder ForWarnEvent() => new NLogEventBuilderAdapter(_inner.ForWarnEvent());

        public void Info(string message) => _inner.Info(message);

        public void Trace(string message) => _inner.Trace(message);

        public void Warn(string message) => _inner.Warn(message);

        #endregion Public Methods
    }
}
