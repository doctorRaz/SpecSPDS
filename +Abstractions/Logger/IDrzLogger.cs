//https://replit.com/@razygraev/Log-Service-interface
using System;

namespace drz.Abstractions.Logger
{
    /// <summary>
    ///
    /// </summary>
    public interface IDrzLogger
    {
        #region Public Properties

        bool IsDebugEnabled { get; }

        bool IsErrorEnabled { get; }

        bool IsFatalEnabled { get; }

        bool IsInfoEnabled { get; }

        bool IsTraceEnabled { get; }

        bool IsWarnEnabled { get; }

        #endregion Public Properties

        #region Public Methods

        void Debug(string message);

        void Error(string message, Exception exception = null);

        void Error(Exception exception, string message = null);

        void Fatal(Exception exception, string message = null);

        void Fatal(string message, Exception exception = null);

        ILogEventBuilder ForDebugEvent();

        ILogEventBuilder ForErrorEvent();

        ILogEventBuilder ForFatalEvent();

        ILogEventBuilder ForInfoEvent();

        ILogEventBuilder ForTraceEvent();

        ILogEventBuilder ForWarnEvent();

        void Info(string message);

        void Trace(string message);

        void Warn(string message);

        #endregion Public Methods
    }
}