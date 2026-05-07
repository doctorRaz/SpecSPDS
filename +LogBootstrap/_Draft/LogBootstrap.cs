using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.IO;




namespace dRz.Cad.Diagnostics
{
    /// <summary>
    /// Конфигурация Nlog
    /// </summary>
    public /*static*/ class LogBootstrap
    {
        //todo переделать на фабрику

        /// <summary>
        /// Initializes
        /// </summary>
        internal static bool Init()
        {
            Exception exNlog = null;
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

                    LoggingConfiguration config = null;
                    try
                    {
                        // Пытаемся использовать внешний конфиг
                        config = LogManager.Configuration;
                    }
                    catch (NLogConfigurationException ex)
                    {
                        exNlog = ex; // Конфиг битый
                        config = null;
                    }


                    if (config == null)
                    {
                        // Создаем с нуля и сразу заполняем всем необходимым
                        LogManager.Configuration = CreateConfiguration();

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
                        //ILogger log = LogManager.GetCurrentClassLogger();
                        ILogger log = NlogFactory.GetLogger<LogBootstrap>();

                        string mode = isProgrammatic ? "Code" : "File";

                        if (exNlog != null)
                        {
                            log.Error(exNlog, exNlog.ToString);
                        }

                        //инфа про адон
                        log.Debug("Config = {0}, AddOn: {1}", mode, InfoDll);

                        //инфа про ос
                        log.Debug("OS: {0}", InfoOs.Current);
                    }
                }

                //исключения поднимаем наверх,
                //без Nlog продолжить работу невозможно,
                //логгер используется везде
                catch (Exception)
                {

                    throw;
                }
            }

            return _initialized;
        }


        private static void ApplyCommonVariables()
        {
            //GDC работает быстрее всего, так что используем его для хранения переменных, которые могут понадобиться в шаблонах и правилах.

            GlobalDiagnosticsContext.Set(LogVar.AppTitle, InfoDll.FilePrefix);
            GlobalDiagnosticsContext.Set(LogVar.LogsDir, InfoDll.AppDataProductLogPath);

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

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        internal static LoggingConfiguration CreateConfiguration()//think переделать на LoggingConfiguration, возврат config, что бы можно было использовать в фабрике
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
                FileName = Path.Combine(InfoDll.AppDataProductLogPath, $"${{shortdate}}_{InfoDll.FilePrefix}.log"),
                ArchiveFileName = Path.Combine(InfoDll.AppDataProductLogPath, $"${{shortdate}}_{InfoDll.FilePrefix}.{{#}}.log"),

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