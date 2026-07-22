using System;
using System.Runtime.CompilerServices;

namespace drz.Abstractions.Logger
{
    /// <summary>
    /// Extensions подробный вывод message<br/>
    ///  [{memberName}:{line}] {message}
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>Debugs the caller.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="line">The line.</param>
        public static void DebugCaller(
                        this IDrzLogger logger,
                        string message,
                        [CallerMemberName] string memberName = "",
                        [CallerLineNumber] int line = 0)
        {
            logger.Debug(FormatCaller(message, memberName, line));
        }

        /// <summary>Traces the caller.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="line">The line.</param>
        public static void TraceCaller(
                        this IDrzLogger logger,
                        string message,
                        [CallerMemberName] string memberName = "",
                        [CallerLineNumber] int line = 0)
        {
            logger.Trace(FormatCaller(message, memberName, line));
        }

        /// <summary>Informations the caller.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="line">The line.</param>
        public static void InfoCaller(
                this IDrzLogger logger,
                string message,
                [CallerMemberName] string memberName = "",
                [CallerLineNumber] int line = 0)
        {
            logger.Info(FormatCaller(message, memberName, line));
        }

        /// <summary>Warns the caller.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="line">The line.</param>
        public static void WarnCaller(
                this IDrzLogger logger,
                string message,
                [CallerMemberName] string memberName = "",
                [CallerLineNumber] int line = 0)
        {
            logger.Warn(FormatCaller(message, memberName, line));
        }

        /// <summary>Errors the caller.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="line">The line.</param>
        public static void ErrorCaller(
                this IDrzLogger logger,
                Exception exception,
                string message = null,
                [CallerMemberName] string memberName = "",
                [CallerLineNumber] int line = 0)
        {
            logger.Error(FormatCaller(message, memberName, line), exception);
        }

        /// <summary>Errors the caller.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="line">The line.</param>
        public static void ErrorCaller(
        this IDrzLogger logger,
        string message,
        Exception exception = null,
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int line = 0)
        {
            logger.Error(exception, FormatCaller(message, memberName, line));
        }

        /// <summary>Fatals the caller.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="line">The line.</param>
        public static void FatalCaller(
        this IDrzLogger logger,
        Exception exception,
        string message = null,
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int line = 0)
        {
            logger.Fatal(FormatCaller(message, memberName, line), exception);
        }

        /// <summary>Fatals the caller.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="line">The line.</param>
        public static void FatalCaller(
        this IDrzLogger logger,
        string message,
        Exception exception = null,
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int line = 0)
        {
            logger.Fatal(exception, FormatCaller(message, memberName, line));
        }

        /// <summary>Formats the caller.</summary>
        /// <param name="message">The message.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        internal static string FormatCaller(
                                string message,
                                string memberName,
                                int line)
        {
            string prefix = $"[{memberName}:{line}]";

            return string.IsNullOrEmpty(message)
                ? prefix
                : $"{prefix} {message}";
        }
    }
}