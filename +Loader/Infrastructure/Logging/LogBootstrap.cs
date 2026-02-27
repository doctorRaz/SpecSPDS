using dRz.Loader.Cad.Infrastructure.Logging.Diagnostics;
using dRz.Loader.Cad.Interfaces;
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
    /// 
    /// </summary>
    internal static /*partial*/ class LogBootstrap
    {

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        internal static void Initialize()
        {

            /*private*/
            IMessageService msg = new MessageService();

            try
            {
                if (_initialized) return;

                // лочим, что бы никто нее влез
                lock (_sync)
                {
                    if (_initialized) return;

                    InternalLoggerHelpers.ConfigureInternalLogger();

                    //конфиг есть
                    if (LogManager.Configuration != null)
                    {
                        //подгружаем GDC
                        SetupGlobalContextHelpers.SetupGlobalContext();

                        LogManager.GetCurrentClassLogger().Debug("Logger started");

                        _initialized = LogManager.Configuration != null;

                        if (_initialized)//иначе программный конфиг
                            return;
                    }

                    LoadConfiguration();

                    _initialized = (LogManager.Configuration != null);

                    if (_initialized)
                    {
                        LogManager.GetCurrentClassLogger().Info("Logger started");
                    }
                    else
                    {
                        msg.ConsoleMessage($"[{nameof(LogBootstrap)}.{nameof(Initialize)}]: Ошибка в конфигурации Logger. Загрузка {LoaderEnvironment.ProductName} будет продолжена");
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        private static void LoadConfiguration()
        {
            string appDataProductLogPath = LoaderEnvironment.AppDataProductLogPath;

            string productTitle = LoaderEnvironment.ProductTitle;

            LoggingConfiguration config = new LoggingConfiguration();

            LogLevel level = ReadLogLevelOnce();

            FileTarget fileTarget = new FileTarget("xmlFile")
            {
                FileName = Path.Combine(appDataProductLogPath, $"${{shortdate}}_{productTitle}.log"),

                ArchiveFileName = Path.Combine(appDataProductLogPath, $"${{shortdate}}_{productTitle}.{{#}}.log"),

                ArchiveEvery = FileArchivePeriod.Day,

                ArchiveAboveSize = 5 * 1024 * 1024,

                MaxArchiveFiles = 10,

                KeepFileOpen = false,

                Layout = CreateXmlLayout()
            };

            // ---------------------------
            // Async wrapper
            // ---------------------------
            AsyncTargetWrapper asyncTarget = new AsyncTargetWrapper(fileTarget)
            {
                QueueLimit = 10000,              // размер очереди
                OverflowAction = AsyncTargetWrapperOverflowAction.Discard,
                BatchSize = 100,
                TimeToSleepBetweenBatches = 50
            };

            config.AddTarget("asyncFile", asyncTarget);

            config.LoggingRules.Add(new LoggingRule("*", level, asyncTarget));

            LogManager.Configuration = config;

        }

        /// <summary>
        /// Чтение уровня лога из файла <br/>
        /// просто  строка<br/>
        /// trace <br/>
        /// debug <br/>
        /// info <br/>
        /// ... <br/>
        /// регистр не важен <br/>
        /// на случай сбоем у юзера, что бы можно было оперативно вкобчить отладку, трассировку в лог
        /// </summary>
        /// <returns>LogLevel</returns>
        private static LogLevel ReadLogLevelOnce()
        {
            // в релизе уровень Error если не переопределено файлом loglevel.txt
#if DEBUG
            LogLevel LevelDefault = LogLevel.Debug;
#else
            LogLevel LevelDefault = LogLevel.Error;
#endif

            try
            {
                // ищем рядом с нашей dll
                string AppDir = LoaderEnvironment.AssemblyDirectory;

                string levelFile = Path.Combine(AppDir, "loglevel.txt");

                if (!File.Exists(levelFile))
                {
                    return LevelDefault;
                }

                foreach (string line in File.ReadLines(levelFile))
                {
                    string text = line.Trim();

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    LogLevel level = LogLevel.FromString(text);

                    return level ?? LevelDefault;
                }

                return LevelDefault;
            }
            catch
            {
                return LevelDefault;
            }
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