using dRz.LogBootstrap.Diagnostics;
using dRz.LogBootstrap.drzNLog;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.IO;

namespace dRz.LogBootstrap.Builder
{
    /// <summary>
    /// Создает NLog фабрику
    /// </summary>
    internal class NLogFactoryBuilder
    {
        /// <summary>Initializes a new instance of the <see cref="NLogFactoryBuilder"/> class.</summary>
        /// <param name="assemblyDirectory">The assembly directory.</param>
        /// <param name="productName">Name of the product.</param>
        /// <param name="productFamily">Product family used in the log file name.</param>
        /// <param name="logsDir">The logs dir.</param>
        internal NLogFactoryBuilder(
                                    string assemblyDirectory, //string assemblyDirectory = _addOnInfo.AssemblyDirectory;
                                    string productName, //string productName = _addOnInfo.Product;
                                    string productFamily, // _addOnInfo.ProductFamily
                                    string logsDir //string logsDir = Path.Combine(_addOnInfo.ProductDataDirectory, "logs");
                                    )
        {
            _assemblyDirectory = assemblyDirectory;
            _productName = productName;
            _productFamily = productFamily;
            _logsDir = logsDir;
        }

        /// _logsDir;<summary>Builds this instance.</summary>
        /// <returns></returns>
        internal LogFactory Build()
        {
            //путь к Diagnostic.Mode
            string diagnosticModePath = Path.Combine(_assemblyDirectory, LogKeys.DiagnosticMode);

            //уровень интернал лога, по умолчанию OFF
            LogLevel internalLogLevel = LogLevelReader.GetLevelFromFile(diagnosticModePath, LogLevel.Off);

            //настраиваем интернал логгер
            InternalLoggerHelpers.ConfigureInternalLogger($"{typeof(NLogFactoryBuilder)}[{_productName}]", internalLogLevel, _productFamily, _logsDir);

            //путь к Log.Level
            string internalLogLevelPath = Path.Combine(_assemblyDirectory, LogKeys.LogLevel);

            //уровень фабрики лога, по умолчанию:
            //      release - Info
            //      debug - Trace
#if DEBUG
            var loglevel = LogLevel.Trace;
#else
            var loglevel = LogLevel.Info;
#endif
            LogLevel currentLevel = LogLevelReader.GetLevelFromFile(internalLogLevelPath, loglevel);

            //фабрика
            LogFactory factory = new()
            {
                //настраиваем фабрику
                Configuration = CreateConfiguration(currentLevel)
            };

            // писать в лог конфигурация фабрики
            LoggingFactoryInfo(factory);

            return factory;
        }

        private readonly string _assemblyDirectory;
        private readonly string _productName;
        private readonly string _productFamily;
        private readonly string _logsDir;

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

        /// <summary>
        /// Creates the configuration.
        /// </summary>
        /// <param name="filePrefix">The file prefix.</param>
        /// <param name="appDataProductLogPath">The application data product log path.</param>
        /// <param name="currentLevel">The current level.</param>
        /// <returns></returns>
        private LoggingConfiguration CreateConfiguration(LogLevel level)
        {
            LoggingConfiguration config = new();

            // Настройка целевого файла
            FileTarget fileTarget = new("file")
            {
                FileName = Path.Combine(_logsDir, $"${{shortdate}}_{_productFamily}.log"),

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
            AsyncTargetWrapper asyncTarget = new(fileTarget)
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
        private void LoggingFactoryInfo(LogFactory factory)
        {
            try
            {
                Logger log = factory.GetLogger(typeof(NLogLoggerFactory).FullName ?? nameof(NLogLoggerFactory));

                //  метод создания события на основе условий
                LogEventBuilder evt = log.ForInfoEvent();

                _ = evt
                    .Message("LogFactory initialized")
                    .Property("ProductName", _productName)
                    .Property("ProductFamily", _productFamily)
                    .Property("LogsDirectory", _logsDir)
                    .Property("LogName", $"YYYY-MM-DD_{_productFamily}.log")
                    .Property("LogLevel", GetEffectiveMinLevel(log));

                if (InternalLogger.LogLevel != LogLevel.Off)
                {
                    _ = evt
                    .Property("InternalLogsDirectory", Path.GetDirectoryName(InternalLogger.LogFile))
                    .Property("InternalLogName", $"YYYY-MM-DD_{_productFamily}_internal.log");
                }

                evt
                    .Property("InternalLogLevel", InternalLogger.LogLevel)
                    .Property("factory_HashCode", factory.GetHashCode().ToString())
                    .Log();
            }
            catch { }// Никогда не роняем приложение из-за диагностики логгера
        }
    }
}