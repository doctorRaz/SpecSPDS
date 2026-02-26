using dRz.Loader.nCad.Infrastructure;
using NLog;
using NLog.Common;
using System;
using System.Globalization;
using System.IO;

namespace dRz.Loader.Cad.Infrastructure.Logging.Diagnostics
{
    internal static class InternalLoggerHelpers
    {

        /// <summary>
        /// Internal logger → Output Window (DEBUG) <br/>
        /// Internal logger → Output file.log
        /// </summary>
        internal static void ConfigureInternalLogger()
        {
#if DEBUG

            InternalLogger.LogLevel = LogLevel.Info;

            //пишем в файл
            InternalLogger.LogFile = LogFile();

            // пишем в output debuger
            InternalLogger.LogWriter = new OutputDebugTextWriter();

            InternalLogger.LogToConsole = true;

            //все исключения
            LogManager.ThrowExceptions = false;

            //ошибки конфига
            LogManager.ThrowConfigExceptions = true;

            InternalLogger.Info($"{LoaderEnvironment.FileName}: InternalLogger Initialize DEBUG");

#else
        InternalLogger.LogLevel = LogLevel.Off;
#endif
        }

        /// <summary>
        /// Logs the file.
        /// </summary>
        /// <returns>путь к лог файлу</returns>
        private static string LogFile()
        {

            string logTimestamp = $"{DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

            return Path.Combine(LoaderEnvironment.AppDataProductLogPath, $"{logTimestamp}_{LoaderEnvironment.FileName}_internal.log");

        }
    }
}