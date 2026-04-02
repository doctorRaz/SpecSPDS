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

        private readonly Func<string> _filePrefixProvider;
        private readonly Func<string> _appDataProductLogPathProvider;

        public LogService(
            Func<string> productNameProvider,
            Func<string> filePrefixProvider,
            Func<string> appDataProductLogPathProvider
            )
        {
            _productNameProvider = productNameProvider ?? throw new ArgumentNullException(nameof(productNameProvider));

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
            factory.GetLogger(type.FullName).Info(productName);
            return factory.GetLogger(type.FullName);
        }

        private LogFactory CreateFactory(string productName)
        {
            string filePrefix = _filePrefixProvider();

            string appDataProductLogPath = _appDataProductLogPathProvider();

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
                exNlog = ex; //todo писать в интернал если Конфиг битый
                config = null;
            }


            if (config == null/*!string.IsNullOrWhiteSpace(configPath) && File.Exists(configPath)*/)//тут проверка конфига на нулл, конфиг если есть подгружается сам
            {//конфг из файла не подтянулся или битый
             //
                factory.Configuration /* LogManager.Configuration*/ = CreateConfiguration(filePrefix, appDataProductLogPath);

                return factory;
            }
            else
            {
                // Конфиг уже есть (nlog.config), просто прокидываем в него 
                // наши пути через переменные
                ApplyCommonVariables(factory,filePrefix, appDataProductLogPath);//todo передавать фабрику
                return factory;
            }

        }


        /// <summary>
        /// Loads the configuration.
        /// </summary>
        private LoggingConfiguration CreateConfiguration(string filePrefix, string appDataProductLogPath)
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

            config.AddTarget("asyncFile", asyncTarget);
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
                    new XmlElement("exception", "${exception:format=ToString}")
                }
            };
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


        private void ApplyCommonVariables(LogFactory factory, string filePrefix,string appDataProductLogPath)
        {
            //GDC работает быстрее всего, так что используем его для хранения переменных, которые могут понадобиться в шаблонах и правилах.
            //todo переделать на variables?? 
            //todo значения стринг а не функц
            GlobalDiagnosticsContext.Set(LogVar.AppTitle, filePrefix);
            GlobalDiagnosticsContext.Set(LogVar.LogsDir, appDataProductLogPath);

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
