using dRz.LogServices.Diagnostics;
using dRz.LogServices.Interfaces;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace dRz.LogServices
{
    public sealed class LogService : ILogService
    {
        private static readonly ConcurrentDictionary<string, LogFactory> _factories = new();

        private readonly Func<string> _productNameProvider;
        private readonly Func<string> _configPathProvider;//x прибить

        private readonly Func<string> _filePrefixProvider;
        private readonly Func<string> _appDataProductLogPathProvider;




        public LogService(
            Func<string> productNameProvider,
            Func<string> configPathProvider,
            Func<string> filePrefixProvider,
            Func<string> appDataProductLogPathProvider
            )
        {
            _productNameProvider = productNameProvider ?? throw new ArgumentNullException(nameof(productNameProvider));
            _configPathProvider = configPathProvider ?? (() => null);

            _filePrefixProvider = filePrefixProvider ?? (() => null);
            _appDataProductLogPathProvider = appDataProductLogPathProvider ?? (() => null);
        }

        public Logger GetLogger<T>() => GetLogger(typeof(T));

        public Logger GetLogger(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            string productName = SafeGetProductName();
            LogFactory factory = _factories.GetOrAdd(productName, CreateFactory);

            return factory.GetLogger(type.FullName);
        }

        private LogFactory CreateFactory(string productName)
        {
            string filePrefix = _filePrefixProvider();

            string appDataProductLogPath = _appDataProductLogPathProvider();

            string configPath = _configPathProvider();//x прибить

            Exception exNlog = null;

            LogFactory factory = null;

            LoggingConfiguration config = null;

            try
            {
                /*LogFactory*/
                factory = new LogFactory();// Пытаемся использовать внешний конфиг

                config = factory.Configuration;
            }
            catch (NLogConfigurationException ex)
            {
                exNlog = ex; // Конфиг битый
                config = null;
            }


            if (config == null/*!string.IsNullOrWhiteSpace(configPath) && File.Exists(configPath)*/)//todo тут проверка конфига на нулл, конфиг если есть подгружается сам
            {//конфг из файла не подтянулся или битый

                //factory.Configuration = CreateFallbackConfig(productName);// Создаем с нуля и сразу заполняем всем необходимым
                                                                          //
                factory.Configuration /* LogManager.Configuration*/ = CreateConfiguration();

                return factory;
            }
            else
            {
                //factory.Configuration = new XmlLoggingConfiguration(configPath);

                // Конфиг уже есть (nlog.config), просто прокидываем в него 
                // наши пути через переменные
                ApplyCommonVariables();
                return factory;
            }

        }


        /// <summary>
        /// Loads the configuration.
        /// </summary>
        private LoggingConfiguration CreateConfiguration()//think переделать на LoggingConfiguration, возврат config, что бы можно было использовать в фабрике
                                                          //todo возможно статик тут не надо?
        {
            LoggingConfiguration config = new LoggingConfiguration();

#if DEBUG
            // Если файла уровня нет — Debug. 
            LogLevel level = LogLevel.Debug;
            // Если файл есть, но пустой —Debug
            string fallbackLevelName = "Trace";
#else
            // Если файла нет — Info. 
            LogLevel level = LogLevel.Info;
            // Если файл есть, но пустой —Trace
            string fallbackLevelName = "Trace";
#endif

            // Если файла уровня нет — Off (ничего не делаем). 
            // Если файл создан, но пустой — Trace (максимум инфы)
            // иначе уровень из файла.
            LogLevel currentLevel = LogLevelReader.GetLevelFromFile(LogKeys.LogLevel, fallbackLevelName);

            if (currentLevel != LogLevel.Off)
            {
                level = currentLevel;
            }



            // Настройка целевого файла
            FileTarget fileTarget = new FileTarget("xmlFile")
            {


                FileName = Path.Combine(_appDataProductLogPathProvider(), $"${{shortdate}}_{_filePrefixProvider()}.log"),
                ArchiveFileName = Path.Combine(_appDataProductLogPathProvider(), $"${{shortdate}}_{_filePrefixProvider()}.{{#}}.log"),

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

            config.AddTarget("asyncFile", asyncTarget);
            config.LoggingRules.Add(new LoggingRule("*", level, asyncTarget));

            //LogManager.Configuration 
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
                    new XmlElement("exception", "${exception:format=ToString}")
                }
            };
        }

        private LoggingConfiguration CreateFallbackConfig(string productName)
        {
            LoggingConfiguration config = new LoggingConfiguration();

            string logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                productName,
                "logs");

            Directory.CreateDirectory(logDir);

            FileTarget fileTarget = new FileTarget("file")
            {
                FileName = Path.Combine(logDir, "${shortdate}.log"),
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}|${exception:format=toString}"
            };

            DebugTarget debugTarget = new DebugTarget("debug");

            config.AddTarget(fileTarget);
            config.AddTarget(debugTarget);

            config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, debugTarget);

            return config;
        }

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


        private void ApplyCommonVariables()
        {
            //GDC работает быстрее всего, так что используем его для хранения переменных, которые могут понадобиться в шаблонах и правилах.

            GlobalDiagnosticsContext.Set(LogVar.AppTitle, _filePrefixProvider);
            GlobalDiagnosticsContext.Set(LogVar.LogsDir, _appDataProductLogPathProvider);

            // Если файла нет — Off (ничего не делаем). 
            // Если файл создан, но пустой — Trace (максимум инфы)
            // иначе уровень из файла.
            LogLevel currentLevel = LogLevelReader.GetLevelFromFile(LogKeys.LogLevel);

            //если офф, то не меняем уровень
            if (currentLevel != LogLevel.Off)
            {
                GlobalDiagnosticsContext.Set(LogVar.LevelMay, currentLevel.ToString());
            }

#if DEBUG || CMD
            //проверка значений  var
            LoggingConfiguration config = LogManager.Configuration;

            if (config.Variables.ContainsKey(LogVar.FinalLevel))
            {
                Layout layot = config.Variables[LogVar.FinalLevel];

                string finalLevel = layot.Render(LogEventInfo.CreateNullEvent());
            }
            if (config.Variables.ContainsKey(LogVar.FinalAppTitle))
            {
                Layout layot = config.Variables[LogVar.FinalAppTitle];

                string finalAppTitle = layot.Render(LogEventInfo.CreateNullEvent());
            }
            if (config.Variables.ContainsKey(LogVar.FinalLogsDir))
            {
                Layout layot = config.Variables[LogVar.FinalLogsDir];

                string finalLogsDir = layot.Render(LogEventInfo.CreateNullEvent());
            }

#endif

            //возможно, стоит вызвать, что бы все обновилось, если конфиг уже был, но может и не нужно, так как мы не меняем правила и шаблоны, а только переменные. Надо протестировать.
            //LogManager.ReconfigExistingLoggers();
        }
    }
}