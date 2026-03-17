using dRz.Loader.Cad.Infrastructure.Info;
using dRz.Loader.Cad.Infrastructure.Logging.Diagnostics;

using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.IO;


#if NC
using dRz.Loader.Cad.Services;
#else
using dRz.SpecSpds.Test.Services;
#endif

namespace dRz.Loader.Cad.Infrastructure.Logging
{
    /// <summary>
    /// Конфигурация Nlog
    /// </summary>
    internal static class LogBootstrap
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Initializes
        /// </summary>
        internal static bool Init()
        {

            if (_initialized)
            {
                return true;
            }

            // лочим, что бы никто нее влез
            lock (_sync)
            {
                if (_initialized)
                {
                    return true;
                }

                bool isProgrammatic = false; // Флаг

                try
                {
                    //диагностика
                    InternalLoggerHelpers.ConfigureInternalLogger();

                    if (LogManager.Configuration == null)
                    {
                        // Создаем с нуля и сразу заполняем всем необходимым
                        LoadConfiguration();
                        isProgrammatic = true;
                    }
                    else
                    {
                        // Конфиг уже есть (nlog.config), просто прокидываем в него 
                        // наши пути через переменные
                        ApplyCommonVariables();
                    }

                    _initialized = LogManager.Configuration != null;

                    if (_initialized)
                    {
                        string mode = isProgrammatic ? "Programm" : "External";
                        //инфа про ос и кад
                        log.Info("Logger started. Mode={0}. App={1}", mode, InfoAdOn.ProductTitle);
                        log.Info("OS={0} {1}", InfoOs.OsDescription, InfoOs.OsArchitecture);

                    }
                }

                //исключения поднимаем наверх,
                //без Nlog продолжить работу невозможно,
                //логгер используется везде
                catch (Exception ex)
                {

                    throw;
                }
            }

            return _initialized;
        }


        private static void ApplyCommonVariables()
        {
            //GDC работает быстрее всего, так что используем его для хранения переменных, которые могут понадобиться в шаблонах и правилах.

            LogLevel currentLevel = ReadLogLevelOnce();

            GlobalDiagnosticsContext.Set("LevelMay", currentLevel.ToString());
            GlobalDiagnosticsContext.Set("AppTitle", InfoAdOn.ProductTitle);
            GlobalDiagnosticsContext.Set("LogsDir", InfoAdOn.AppDataProductLogPath);

            //возможно, стоит вызвать, что бы все обновилось, если конфиг уже был, но может и не нужно, так как мы не меняем правила и шаблоны, а только переменные. Надо протестировать.
            //LogManager.ReconfigExistingLoggers();
        }

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        private static void LoadConfiguration()
        {
            LoggingConfiguration config = new LoggingConfiguration();

            LogLevel level = ReadLogLevelOnce();

            // Настройка целевого файла
            FileTarget fileTarget = new FileTarget("xmlFile")
            {
                FileName = Path.Combine(InfoAdOn.AppDataProductLogPath, $"${{shortdate}}_{InfoAdOn.ProductTitle}.log"),
                ArchiveFileName = Path.Combine(InfoAdOn.AppDataProductLogPath, $"${{shortdate}}_{InfoAdOn.ProductTitle}.{{#}}.log"),

                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveAboveSize = 5 * 1024 * 1024,
                MaxArchiveFiles = 10,

                KeepFileOpen = true,
                OpenFileCacheTimeout = 30,

                Layout = CreateXmlLayout(),

            };

            // ---------------------------
            // Async wrapper
            // ---------------------------
            AsyncTargetWrapper asyncTarget = new AsyncTargetWrapper(fileTarget)
            {
                QueueLimit = 10000,              // размер очереди
                OverflowAction = AsyncTargetWrapperOverflowAction.Discard,
                BatchSize = 500,
                TimeToSleepBetweenBatches = 10,
            };

            config.AddTarget("asyncFile", asyncTarget);
            config.LoggingRules.Add(new LoggingRule("*", level, asyncTarget));

            LogManager.Configuration = config;

        }

        /// <summary>
        /// Reads the log level once.
        /// </summary>
        /// <returns></returns>
        private static LogLevel ReadLogLevelOnce()
        {
            // Если файла нет — Info. 
            // Если файл есть, но пустой — тоже Info
            return LogLevelReader.GetLevelFromFile("log.level", LogLevel.Info, LogLevel.Info);
        }

        /// <summary>
        /// XML layout
        /// </summary>
        /// <returns></returns>

        private static XmlLayout CreateXmlLayout()
        {
            return new XmlLayout
            {
                IncludeEventProperties = true,
                IndentXml = true,
                MaxRecursionLimit = 10,
                ElementName = "logevent",

                Attributes =
                {
                    new XmlAttribute("time", "${longdate}"),
                    new XmlAttribute("level", "${level:uppercase=true}"),
                    new XmlAttribute("logger", "${logger}"),
                },

                Elements =
                {
                    new XmlElement("message", "${message}"),
                    new XmlElement("exception", "${exception:format=ToString}")
                }
            };
        }

        /// <summary>
        /// The initialized
        /// </summary>
        private static bool _initialized;

        /// <summary>
        /// The synchronize
        /// </summary>
        private static readonly object _sync = new();

    }
}