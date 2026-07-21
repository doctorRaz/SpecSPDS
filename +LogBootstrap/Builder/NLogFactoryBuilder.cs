using drz.Abstractions.Infrastructure;
using drz.LogBootstrap.Diagnostics;
using drz.LogBootstrap.drzNLog;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.IO;

namespace drz.LogBootstrap.Builder
{
    /// <summary>
    /// Создает NLog фабрику
    /// </summary>
    internal class NLogFactoryBuilder
    {
        private readonly IAddOnInfo _addOnInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogFactoryBuilder"/> class.
        /// </summary>
        /// <param name="addOnInfo">The add on information.</param>
        internal NLogFactoryBuilder(IAddOnInfo addOnInfo)
        {
            _addOnInfo = addOnInfo;
        }

        /// <summary>Builds this instance.</summary>
        /// <returns></returns>
        internal LogFactory Build()
        {
            string assemblyDirectory = _addOnInfo.AssemblyDirectory;
            string productName = _addOnInfo.ProductName;

            //путь к Diagnostic.Mode
            string baseDirDiagnostyc = Path.Combine(assemblyDirectory, LogKeys.DiagnosticMode);

            LogLevel requestedLevel = LogLevelReader.GetLevelFromFile(baseDirDiagnostyc);

            InternalLoggerHelpers.ConfigureInternalLogger($"{typeof(NLogLoggerFactory).FullName}.{productName}", requestedLevel);

            string logName = $"{productName}{_addOnInfo.CadCode}";// ${shortdate}_{logName}.log;

            string logsDir = _addOnInfo.AppDataProductLogPath;// Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), productName, "logs"); // _appDataProductLogPathProvider();

            Exception configException = null;

            LogFactory factory = null;

            LoggingConfiguration config = null;

            bool isFallback = false;

            try
            {
                factory = new LogFactory();// Пытаемся использовать внешний конфиг

                config = factory.Configuration;
            }
            catch (NLogConfigurationException ex)
            {
                configException = ex; //потом запишем в лог этой фабрики

                config = null;
            }

            //путь к Log.Level
            string baseDirLogLevel = Path.Combine(assemblyDirectory, LogKeys.LogLevel);

            LogLevel currentLevel = LogLevelReader.GetLevelFromFile(baseDirLogLevel);

            if (config == null || config.AllTargets.Count == 0 || config.LoggingRules.Count == 0)//тут проверка конфига на нулл, конфиг если есть подгружается сам
            {
                //конфг из файла не подтянулся или битый
                isFallback = true;

                factory.Configuration = CreateConfiguration(logName, logsDir, currentLevel);
            }
            else
            {
                // Конфиг уже есть (nlog.config), просто прокидываем в него
                // наши пути через переменные
                ApplyCommonVariables(factory, logName, logsDir, currentLevel);
            }

            // писать в лог результат создания фабрики 
            // 🔴 ВАЖНО: только после того как конфиг установлен
            WriteFactoryDiagnostics(factory, productName, isFallback, configException);

            return factory;
        }

        /// <summary>
        /// Creates the configuration.
        /// </summary>
        /// <param name="filePrefix">The file prefix.</param>
        /// <param name="appDataProductLogPath">The application data product log path.</param>
        /// <param name="currentLevel">The current level.</param>
        /// <returns></returns>
        private LoggingConfiguration CreateConfiguration(string filePrefix, string appDataProductLogPath, LogLevel currentLevel)
        {
            LoggingConfiguration config = new LoggingConfiguration();

#if DEBUG
            // Если файла уровня нет — Debug.
            LogLevel level = LogLevel.Debug;
#else
            // Если файла нет — Info.
            LogLevel level = LogLevel.Info;
            // Если файл есть, но пустой —Trace
            //string fallbackLevelName = "Trace";
#endif

            // Если файла уровня нет — Off (ничего не делаем).
            // Если файл создан, но пустой — Trace (максимум инфы)
            // иначе уровень из файла.
            //LogLevel currentLevel = LogLevelReader.GetLevelFromFile(LogKeys.LogLevel, fallbackLevelName);

            if (currentLevel != LogLevel.Off)
            {
                level = currentLevel;
            }

            // Настройка целевого файла
            FileTarget fileTarget = new FileTarget("file")
            {
                FileName = Path.Combine(appDataProductLogPath, $"${{shortdate}}_{filePrefix}.log"),

                ArchiveFileName = Path.Combine(appDataProductLogPath, $"${{shortdate}}_{filePrefix}.{{#}}.log"),

                ArchiveEvery = FileArchivePeriod.Day,
                //ArchiveAboveSize = 5 * 1024 * 1024,
                MaxArchiveFiles = 10,

                KeepFileOpen = false,

                OpenFileCacheTimeout = 10,

                Layout = CreateXmlLayout(),
            };

            // ---------------------------
            // Async wrapper
            // ---------------------------
            AsyncTargetWrapper asyncTarget = new AsyncTargetWrapper(fileTarget)
            {
                QueueLimit = 10000,              // размер очереди
                OverflowAction = AsyncTargetWrapperOverflowAction.Block,
                BatchSize = 500,
                TimeToSleepBetweenBatches = 50,
            };

            config.AddTarget("async", asyncTarget);
            config.LoggingRules.Add(new LoggingRule("*", level, asyncTarget));

            return config;
        }

        /// <summary>
        /// Applies the common variables.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="appTitle">The application title.</param>
        /// <param name="logsDir">The logs dir.</param>
        /// <param name="currentLevel">The current level.</param>
        private void ApplyCommonVariables(LogFactory factory, string appTitle, string logsDir, LogLevel currentLevel)
        {
            LoggingConfiguration config = factory.Configuration;

            if (factory.Configuration == null)
            {
                return;
            }

            /*factory.Configuration*/
            config.Variables["AppTitle"] = appTitle;
            /*factory.Configuration*/
            config.Variables["LogsDir"] = logsDir;

            // Если файла нет — Off (ничего не делаем).
            // Если файл создан, но пустой — Trace (максимум инфы)
            // иначе уровень из файла.
            //LogLevel currentLevel = LogLevelReader.GetLevelFromFile(LogKeys.LogLevel);

            //если офф, то не меняем уровень
            if (currentLevel != LogLevel.Off)
            {
                /*factory.Configuration*/
                config.Variables["LevelMay"] = currentLevel.ToString();
            }

            factory.ReconfigExistingLoggers();

#if DEBUG || TEST
            //проверка значений  var

            if (config.Variables.ContainsKey(LogVar.LevelMay))
            {
                Layout layot = config.Variables[LogVar.LevelMay];

                string finalLevel = layot.Render(LogEventInfo.CreateNullEvent());
            }
            if (config.Variables.ContainsKey(LogVar.AppTitle))
            {
                Layout layot = config.Variables[LogVar.AppTitle];

                string finalAppTitle = layot.Render(LogEventInfo.CreateNullEvent());
            }
            if (config.Variables.ContainsKey(LogVar.LogsDir))
            {
                Layout layot = config.Variables[LogVar.LogsDir];

                string finalLogsDir = layot.Render(LogEventInfo.CreateNullEvent());
            }
#endif
        }

        /// <summary>
        /// Writes the factory diagnostics.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="productName">Name of the product.</param>
        /// <param name="isFallback">if set to <c>true</c> [is fallback].</param>
        /// <param name="configException">The configuration exception.</param>
        private void WriteFactoryDiagnostics(LogFactory factory,
                                             string productName,
                                             bool isFallback,
                                             Exception configException)
        {
            try
            {
                Logger log = factory.GetLogger(typeof(NLogLoggerFactory).FullName);

                // Internal log level
                LogLevel internalLevel = InternalLogger.LogLevel;

                // Factory min level
                LogLevel effectiveLevel = GetEffectiveMinLevel(log);

                string msg = $"LogFactory initialized: {productName};\n" +
                                $"ConfigSource: {(isFallback ? "Fallback (programmatic)" : "External (nlog.config)")};\n" +
                                $"EffectiveMinLevel: {effectiveLevel};\n" +
                                $"InternalLoggerLevel: {internalLevel}";

                if (isFallback)
                {
                    log.Warn(msg);
                }
                else
                {
                    log.Info(msg);
                }

                if (configException != null)
                {
                    log.Error(configException, configException.Message);
                }
            }
            catch { }// Никогда не роняем приложение из-за диагностики логгера
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
                    new XmlAttribute("pid", "${processid}"),
                    new XmlAttribute("fullName", "${processname:fullName=true}"),
                },

                Elements =
                {
                    new XmlElement("message", "${message}"),
                    new XmlElement("exception", "${exception:format=ToString:innerFormat=ToString:maxInnerExceptionLevel=10}")
                }
            };
        }

        /// <summary>
        /// Gets the effective minimum level.
        /// </summary>
        /// <param name="logger">The log.</param>
        /// <returns></returns>
        private static LogLevel GetEffectiveMinLevel(Logger logger)
        {
            foreach (LogLevel level in LogLevel.AllLevels) // Trace → Fatal
            {
                if (logger.IsEnabled(level))
                {
                    return level;
                }
            }

            return LogLevel.Off;
        }
    }
}