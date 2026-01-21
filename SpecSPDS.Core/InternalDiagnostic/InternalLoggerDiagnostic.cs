using NLog;
using NLog.Common;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

//todo только для отладки загрузки nlog конфигурации
// скопировать в dRz.SpecSPDS.Cad.Bootstrap

namespace dRz.SpecSPDS.Core.InternalDiagnostic
{
    public class InternalLoggerDiagnostic
    {
        /// <summary>
        /// внутренняя инициализация логгера NLog для отладки самого NLog
        /// </summary>
        public static void InternalLoggerInit()
        {
            Debug.WriteLine($"OR\t{Assembly.GetEntryAssembly()?.Location ?? Assembly.GetExecutingAssembly().Location}");

            Debug.WriteLine($"GetExecutingAssembly\t{Assembly.GetExecutingAssembly().Location}");

            Debug.WriteLine($"GetEntryAssembly\t{Assembly.GetEntryAssembly().Location}");

          

            #region FilePathInternalLogger

            string? moduleName = Assembly.GetEntryAssembly()?.GetName().Name ?? Assembly.GetExecutingAssembly().GetName().Name;

            string logTimestamp = $"{DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}";

            string? dllDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location ?? Assembly.GetExecutingAssembly().Location);
            

            #endregion

            #region InternalLogger configure

            InternalLogger.LogLevel = LogLevel.Info;

            InternalLogger.LogWriter = new DebugTextWriter();
            
            LogManager.ThrowExceptions = true;

            LogManager.ThrowConfigExceptions = true;

            InternalLogger.LogFile = Path.Combine(dllDir, "logs", $"{logTimestamp}_{moduleName}_internal.log");
            InternalLogger.Info($"[{moduleName}]: InternalLogger.Initialize()");

            #endregion
        }
    }
}