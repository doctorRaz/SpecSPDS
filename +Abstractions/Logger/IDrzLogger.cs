//https://replit.com/@razygraev/Log-Service-interface
using System;

namespace drz.Abstractions.Logger
{
    /// <summary> Provides logging interface and utility functions. </summary>
    public interface IDrzLogger
    {
        #region Public Properties

        /// <summary>Gets a value indicating whether this instance is debug enabled.</summary>
        /// <value><c>true</c> if this instance is debug enabled; otherwise, <c>false</c>.</value>
        bool IsDebugEnabled { get; }

        /// <summary>Gets a value indicating whether this instance is error enabled.</summary>
        /// <value><c>true</c> if this instance is error enabled; otherwise, <c>false</c>.</value>
        bool IsErrorEnabled { get; }

        /// <summary>Gets a value indicating whether this instance is fatal enabled.</summary>
        /// <value><c>true</c> if this instance is fatal enabled; otherwise, <c>false</c>.</value>
        bool IsFatalEnabled { get; }

        /// <summary>Gets a value indicating whether this instance is information enabled.</summary>
        /// <value><c>true</c> if this instance is information enabled; otherwise, <c>false</c>.</value>
        bool IsInfoEnabled { get; }

        /// <summary>Gets a value indicating whether this instance is trace enabled.</summary>
        /// <value><c>true</c> if this instance is trace enabled; otherwise, <c>false</c>.</value>
        bool IsTraceEnabled { get; }

        /// <summary>Gets a value indicating whether this instance is warning enabled.</summary>
        /// <value><c>true</c> if this instance is warning enabled; otherwise, <c>false</c>.</value>
        bool IsWarnEnabled { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>Debugs the specified message.</summary>
        /// <param name="message">The message.</param>
        /// <param name="line">The line.</param>
        /// <param name="memberName">Name of the member.</param>
        void Debug(string message);

        /// <summary>Logs an error message.</summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        void Error(string message, Exception exception = null);

        /// <summary>Logs an error exception.</summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        void Error(Exception exception, string message = null);

        /// <summary>Fatals the specified exception.</summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        void Fatal(Exception exception, string message = null);

        /// <summary>Fatals the specified message.</summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        void Fatal(string message, Exception exception = null);

        /// <summary>Fors the debug event.</summary>
        /// <returns></returns>
        ILogEventBuilder ForDebugEvent();

        /// <summary>Fors the error event.</summary>
        /// <returns></returns>
        ILogEventBuilder ForErrorEvent();

        /// <summary>Fors the fatal event.</summary>
        /// <returns></returns>
        ILogEventBuilder ForFatalEvent();

        /// <summary>Fors the information event.</summary>
        /// <returns></returns>
        ILogEventBuilder ForInfoEvent();

        /// <summary>Fors the trace event.</summary>
        /// <returns></returns>
        ILogEventBuilder ForTraceEvent();

        /// <summary>Fors the warning event.</summary>
        /// <returns></returns>
        ILogEventBuilder ForWarnEvent();

        /// <summary>Informations the specified message.</summary>
        /// <param name="message">The message.</param>
        void Info(string message);

        /// <summary>Traces the specified message.</summary>
        /// <param name="message">The message.</param>
        void Trace(string message);

        /// <summary>Warns the specified message.</summary>
        /// <param name="message">The message.</param>
        void Warn(string message);

        #endregion Public Methods
    }
}