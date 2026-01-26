using NLog;
using NLog.Common;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

//todo только для отладки загрузки nlog конфигурации
// скопировать в drz.Cad.Cad.InternalDiagnostic

namespace dRz.Loader.Cad.InternalDiagnostic
{
    public class InternalLoggerDiagnostic
    {
        /// <summary>
        /// внутренняя инициализация логгера NLog, только для отладки самого NLog
        /// </summary>
        /// <param name="OnlyDebugTextWriter">только вывод в отладку</param>
        public InternalLoggerDiagnostic(string? message= null)
        {
            if (logLevel == LogLevel.Off)//диагностика не включена
            {
                //включаем
                Writer();
                Init();

            }
            else
            {
                //включена тогда только писатель
                Writer();

            }

            //пишем в интернал лог
            if (!string.IsNullOrWhiteSpace(message))
            {
                InternalLogger.Warn($"{message}");
            }

        }

        public static void Writer()
        {
            InternalLogger.LogWriter = new DebugTextWriter();
        }


         void Init()
        {
            #region InternalLogger configure

            //смотрим все события
            InternalLogger.LogLevel = LogLevel.Trace;

            //пишем в файл
            InternalLogger.LogFile = LogFile();

            //все исключения
            LogManager.ThrowExceptions = false;

            //ошибки конфига
            LogManager.ThrowConfigExceptions = false;

            InternalLogger.Info($"{moduleName}: InternalLogger Initialize manual");

            #endregion
        }



        #region Log Diagnostic что б посмотреть в отладке

        LogLevel logLevel => InternalLogger.LogLevel;

        string? logFile => InternalLogger.LogFile;

        bool throwExceptions => LogManager.ThrowExceptions;

        bool? throwConfigExceptions => LogManager.ThrowConfigExceptions;

        #endregion

        Assembly assembly => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        string? moduleName => assembly.GetName().Name;

        string LogFile()
        {
            #region FilePathInternalLogger

            string logTimestamp = $"{DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}";

            string? dllDir = Path.GetDirectoryName(assembly.Location);

            return Path.Combine(dllDir, "logs", $"{logTimestamp}_{moduleName}_internal.log");

            #endregion
        }
    }
}