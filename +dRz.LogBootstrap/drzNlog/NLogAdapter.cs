//https://replit.com/@razygraev/Log-Service-interface

using drz.Abstractions.Logger;
using NLog;
using System;

namespace drz.LogServices.drzNlog
{
    internal sealed class NLogAdapter : IDrzLogger
    {
        private readonly NLog.Logger _inner;

        internal NLogAdapter(NLog.Logger inner) => _inner = inner;

        public bool IsTraceEnabled => _inner.IsTraceEnabled;
        public bool IsDebugEnabled => _inner.IsDebugEnabled;
        public bool IsInfoEnabled => _inner.IsInfoEnabled;
        public bool IsWarnEnabled => _inner.IsWarnEnabled;
        public bool IsErrorEnabled => _inner.IsErrorEnabled;
        public bool IsFatalEnabled => _inner.IsFatalEnabled;

        public void Trace(string message) => _inner.Trace(message);

        public void Debug(string message) => _inner.Debug(message);

        public void Info(string message) => _inner.Info(message);

        public void Warn(string message) => _inner.Warn(message);

        public void Error(string message, Exception exception = null) => _inner.Error(exception, message);

        public void Fatal(string message, Exception exception = null) => _inner.Fatal(exception, message);

        public ILogEventBuilder ForTraceEvent() => new NLogEventBuilderAdapter(_inner.ForTraceEvent());

        public ILogEventBuilder ForDebugEvent() => new NLogEventBuilderAdapter(_inner.ForDebugEvent());

        public ILogEventBuilder ForInfoEvent() => new NLogEventBuilderAdapter(_inner.ForInfoEvent());

        public ILogEventBuilder ForWarnEvent() => new NLogEventBuilderAdapter(_inner.ForWarnEvent());

        public ILogEventBuilder ForErrorEvent() => new NLogEventBuilderAdapter(_inner.ForErrorEvent());

        public ILogEventBuilder ForFatalEvent() => new NLogEventBuilderAdapter(_inner.ForFatalEvent());
    }
}
