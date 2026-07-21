using drz.Abstractions.Logger;
using drz.LogServices.Diagnostics;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Concurrent;
using System.IO;

//https://replit.com/@razygraev/Log-Service-interface

namespace drz.LogServices.drzNlog
{
    /// <summary>
    /// Log Service
    /// </summary>
    /// <seealso cref="IDrzLoggerFactory" />
    public sealed class NLogLoggerFactory : IDrzLoggerFactory
    {
        private static readonly ConcurrentDictionary<string, LogFactory> _factories = new();

        private readonly Func<string> _productNameProvider;

        private readonly Func<string> _assemblyDirectoryProvider;

        //private readonly string _assemblyDirectory;

        private readonly IEnvironmentInfoProvider _envInfoProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogService"/> class.
        /// </summary>
        /// <param name="productNameProvider">The product name provider.</param>
        /// <param name="assemblyDirectoryProvider">The assembly directory provider.</param>
        /// <param name="envInfoProvider">The env information provider.</param>
        /// <exception cref="System.ArgumentNullException">
        /// productNameProvider
        /// or
        /// assemblyDirectoryProvider
        /// </exception>
        public NLogLoggerFactory(
            Func<string> productNameProvider,
            Func<string> assemblyDirectoryProvider,
            IEnvironmentInfoProvider envInfoProvider = null)

        {
            _productNameProvider = productNameProvider ?? throw new ArgumentNullException(nameof(productNameProvider));

            _assemblyDirectoryProvider = assemblyDirectoryProvider ?? throw new ArgumentNullException(nameof(assemblyDirectoryProvider));

            _envInfoProvider = envInfoProvider;
        }

        /// <summary> Возвращает логгер для указанного продукта..</summary>
        /// <typeparam name="T">владелец логера</typeparam>
        /// <param name="productName">название продукта.</param>
        /// <returns>Экземпляр логгера</returns>
        /// <exception cref="System.ArgumentNullException">
        /// productName
        /// or
        /// type
        /// </exception>
        public IDrzLogger GetLogger<T>(string productName)
        {
            //todo проверить логер из дочерних сборок
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentNullException(nameof(productName));

            Type type = typeof(T);

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            LogFactory factory = _factories.GetOrAdd(productName, CreateFactory);

            return new NLogAdapter(factory.GetLogger(type.FullName));
        }


        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IDrzLogger GetLogger<T>() => GetLogger(typeof(T));

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">type</exception>
        public IDrzLogger GetLogger(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            string productName = SafeGetProductName();
            LogFactory factory = _factories.GetOrAdd(productName, CreateFactory);

            return new NLogAdapter(factory.GetLogger(type.FullName));
        }

        /// <summary>
        /// Creates the factory.
        /// </summary>
        /// <param name="productName">Name of the product.</param>
        /// <returns></returns>
        private LogFactory CreateFactory(string productName)
        {
            string assemblyDirectory = SafeGetAssemblyDirectory();

            //путь к Diagnostic.Mode
            string baseDirDiagnostyc = Path.Combine(assemblyDirectory, LogKeys.DiagnosticMode);

            LogLevel requestedLevel = LogLevelReader.GetLevelFromFile(baseDirDiagnostyc);

            InternalLoggerHelpers.ConfigureInternalLogger($"{typeof(NLogLoggerFactory).FullName}.{productName}", requestedLevel);

            string logName = productName;// ${shortdate}_{logName}.log;

            string logsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), productName, "logs"); // _appDataProductLogPathProvider();

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
                configException = ex; //потом запишем в  лог этой фабрики

                config = null;
            }

            //todo получить Level тут!!!

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

            // писать в лог о системе о каде как этот лог сделан
            // 🔴 ВАЖНО: после того как конфиг установлен
            WriteFactoryDiagnostics(factory, productName, isFallback, configException);

            return factory;
        }

        private string SafeGetAssemblyDirectory()
        {
            try
            {
                string assemblyDirectory = _assemblyDirectoryProvider();
                return string.IsNullOrWhiteSpace(assemblyDirectory) ? string.Empty : assemblyDirectory;
            }
            catch
            {
                return string.Empty;
            }
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
                    log.Debug(msg);
                }

                if (_envInfoProvider != null)
                {
                    log.Debug($"{_envInfoProvider.GetSummary()}");
                }

                if (configException != null)
                {
                    log.Error(configException, configException.Message);
                }
            }
            catch { }// Никогда не роняем приложение из-за диагностики логгера
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
        /// Safes the name of the get product.
        /// </summary>
        /// <returns></returns>
        private string SafeGetProductName()
        {
            try
            {
                string name = _productNameProvider();
                return string.IsNullOrWhiteSpace(name) ? "UnknownProduct" : name;
            }
            catch
            {
                return "UnknownProduct";
            }
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



    }
}