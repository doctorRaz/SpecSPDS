using dRz.Abstractions.Infrastructure;
using dRz.LogBootstrap.Diagnostics;
using dRz.LogBootstrap.drzNLog;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace dRz.LogBootstrap.Builder
{
    /// <summary>
    /// Создает NLog фабрику
    /// </summary>
    internal class NLogFactoryBuilder
    {
        private readonly IAddOnInfo _addOnInfo;//todo передавать только нужные параметры

        #region КОНТРАКТ

        // NLogFactoryBuilder не должен зависеть от всего IAddOnInfo.
        // Ему нужны только четыре значения: каталог сборки, имя продукта,
        // семейство продукта и каталог логов.
        // Передача этих параметров напрямую изолирует builder от контракта IAddOnInfo.

        #endregion КОНТРАКТ

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
            string productName = _addOnInfo.Product;

            //путь к Diagnostic.Mode
            // фабрика сама определяет как называть файлы
            string logName = _addOnInfo.ProductFamily;

            // сама решает в какой каталог складывать логи
            string logsDir = Path.Combine(_addOnInfo.ProductDataDirectory, "logs");

            //путь к Diagnostic.Mode
            string baseDirDiagnostic = Path.Combine(assemblyDirectory, LogKeys.DiagnosticMode);

            //уровень интернал лога, по умолчанию OFF
            LogLevel internalLogLevel = LogLevelReader.GetLevelFromFile(baseDirDiagnostic, LogLevel.Off);

            //настраиваем интернал логгер
            InternalLoggerHelpers.ConfigureInternalLogger($"{typeof(NLogFactoryBuilder)}[{productName}]", internalLogLevel, logName, logsDir);

            //путь к Log.Level
            string baseDirLogLevel = Path.Combine(assemblyDirectory, LogKeys.LogLevel);

            //уровень фабрики лога, по умолчанию Innfo
            LogLevel currentLevel = LogLevelReader.GetLevelFromFile(baseDirLogLevel, LogLevel.Info);

            //фабрика
            LogFactory factory = new LogFactory();

            //настраиваем фабрику
            factory.Configuration = CreateConfiguration(logName, logsDir, currentLevel);

            // писать в лог конфигурация фабрики
            LoggingFactoryInfo(factory, productName, logName, logsDir);

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

            LogLevel level = currentLevel;

            // Настройка целевого файла
            FileTarget fileTarget = new FileTarget("file")
            {
                FileName = Path.Combine(appDataProductLogPath, $"${{shortdate}}_{filePrefix}.log"),

                ArchiveEvery = FileArchivePeriod.Day,

                ArchiveSuffixFormat = ".{0}",

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

        /// <summary>Loggings the factory information.</summary>
        /// <param name="factory">The factory.</param>
        /// <param name="productName">Name of the product.</param>
        /// <param name="logName">Name of the log.</param>
        /// <param name="logDir">The log dir.</param>
        /// <param name="isFallback">if set to <c>true</c> [is fallback].</param>
        /// <param name="configException">The configuration exception.</param>
        private void LoggingFactoryInfo(LogFactory factory,
                                             string productName,
                                             string logName,
                                             string logDir)
        {
            try
            {
                Logger log = factory.GetLogger(typeof(NLogLoggerFactory).FullName);

                //  метод создания события на основе условий
                LogEventBuilder evt = log.ForInfoEvent();

                evt
                    .Message("LogFactory initialized")
                    .Property("ProductName", productName)
                    .Property("ProductFamily", _addOnInfo.ProductFamily)
                    .Property("LogsDirectory", logDir)
                    .Property("LogName", $"YYYY-MM-DD_{logName}.log")
                    .Property("LogLevel", GetEffectiveMinLevel(log));

                if (InternalLogger.LogLevel != LogLevel.Off)
                {
                    evt
                    .Property("InternalLogsDirectory", Path.GetDirectoryName(InternalLogger.LogFile))
                    .Property("InternalLogName", $"YYYY-MM-DD_{logName}_internal.log");
                }

                evt
                    .Property("InternalLogLevel", InternalLogger.LogLevel)
                    .Property("factory_HashCode", factory.GetHashCode().ToString())
                    .Log();
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