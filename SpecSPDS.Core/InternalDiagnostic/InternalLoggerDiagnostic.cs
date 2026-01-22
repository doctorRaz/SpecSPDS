using NLog;
using NLog.Common;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

//todo только для отладки загрузки nlog конфигурации
// скопировать в drz.Cad.Cad.InternalDiagnostic

namespace dRz.SpecSPDS.Core.InternalDiagnostic
{
    public class InternalLoggerDiagnostic
    {
        /// <summary>
        /// внутренняя инициализация логгера NLog, только для отладки самого NLog
        /// </summary>
        public static void InternalLoggerInit()
        {
            #region FilePathInternalLogger

            string logTimestamp = $"{DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}";

            Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            string? moduleName = assembly.GetName().Name;
            // Assembly.GetEntryAssembly()?.GetName().Name ?? Assembly.GetExecutingAssembly().GetName().Name;

            string? dllDir = Path.GetDirectoryName(assembly.Location);
            //Assembly.GetEntryAssembly()?.Location ?? Assembly.GetExecutingAssembly().Location);

            string? logFile = Path.Combine(dllDir, "logs", $"{logTimestamp}_{moduleName}_internal.log");

            #endregion

            #region InternalLogger configure

            var ll = InternalLogger.LogLevel;
            var lf = InternalLogger.LogFile;
            var te = LogManager.ThrowExceptions;
            var tce = LogManager.ThrowConfigExceptions;


            InternalLogger.LogLevel = LogLevel.Trace;

            InternalLogger.LogFile = logFile;

            InternalLogger.LogWriter = new DebugTextWriter();

            LogManager.ThrowExceptions = false;

            LogManager.ThrowConfigExceptions = true;

            InternalLogger.Info($"{moduleName}: InternalLogger.Initialize");

            #endregion
        }
    }
}