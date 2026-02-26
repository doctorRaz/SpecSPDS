using dRz.Loader.nCad.Infrastructure;
using NLog;
using NLog.Common;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

//todo только для отладки загрузки nlog конфигурации
// скопировать в drz.nCad.nCad.InternalDiagnostic

namespace dRz.SpecSpds.Test.Infrastructure.InternalDiagnostic
{
    public class InternalLoggerDiagnostic
    {
        /// <summary>
        /// внутренняя инициализация логгера NLog, только для отладки самого NLog
        /// </summary>
        /// <param name="OnlyDebugTextWriter">только вывод в отладку</param>
        public InternalLoggerDiagnostic(string? message = null)
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


        private void Init()
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

        private LogLevel logLevel => InternalLogger.LogLevel;

        private string? logFile => InternalLogger.LogFile;

        private bool throwExceptions => LogManager.ThrowExceptions;

        private bool? throwConfigExceptions => LogManager.ThrowConfigExceptions;

        #endregion

        private Assembly assembly => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

        private string? moduleName => assembly.GetName().Name;

        private string LogFile()
        {
            #region FilePathInternalLogger

            string logTimestamp = $"{DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}";

            string appDir = LoaderEnvironment.AppDataProductLogPath;

            return Path.Combine(appDir, $"{logTimestamp}_{moduleName}_internal.log");

            #endregion
        }
    }
}