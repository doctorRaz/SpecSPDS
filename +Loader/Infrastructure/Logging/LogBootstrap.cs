using dRz.Loader.Cad.Infrastructure.Logging.Diagnostics;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System.IO;
using System;



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

        /// <summary>
        /// Initializes
        /// </summary>
        internal static bool Init()
        {

            if (_initialized) return true;

            // лочим, что бы никто нее влез
            lock (_sync)
            {
                if (_initialized) return true;

                try
                {
                    //диагностика
                    InternalLoggerHelpers.ConfigureInternalLogger();

                    if (LogManager.Configuration == null)
                    {
                        // Создаем с нуля и сразу заполняем всем необходимым
                        LoadConfiguration();
                    }
                    else
                    {
                        // Конфиг уже есть (nlog.config), просто прокидываем в него 
                        // наши пути через переменные
                        ApplyCommonVariables(LogManager.Configuration);
                    }


                    _initialized = LogManager.Configuration != null;

                    if (_initialized)
                    {

                        LogManager.GetCurrentClassLogger().Info("Logger started. App: {0}", LoaderEnvironment.ProductTitle);
                    }

                }

                //исключения поднимаем наверх,
                //без Nlog продолжить работу невозможно,
                //логгер используется везде
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"Critical Logger Failure: {ex}");
                    throw;
                }
            }

            return _initialized;
        }


        private static void ApplyCommonVariables(LoggingConfiguration? config)
        {
            //#if DEBUG
            //            GlobalDiagnosticsContext.Set("DateCreate", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture));
            //#endif
            //config.Variables["LevelMay"] = ReadLogLevelOnce().ToString();
            //config.Variables["LogsDir"] = LoaderEnvironment.AppDataProductLogPath;
            //config.Variables["AppTitle"] = LoaderEnvironment.ProductTitle;


            GlobalDiagnosticsContext.Set("LevelMay", ReadLogLevelOnce().ToString());
            GlobalDiagnosticsContext.Set("AppTitle", LoaderEnvironment.ProductTitle);
            GlobalDiagnosticsContext.Set("LogsDir", LoaderEnvironment.AppDataProductLogPath);

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
                FileName = Path.Combine(LoaderEnvironment.AppDataProductLogPath, $"${{shortdate}}_{LoaderEnvironment.ProductTitle}.log"),
                ArchiveFileName = Path.Combine(LoaderEnvironment.AppDataProductLogPath, $"${{shortdate}}_{LoaderEnvironment.ProductTitle}.{{#}}.log"),

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
                TimeToSleepBetweenBatches = 1,
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
            LogLevel levelDefault = LogLevel.Error;
#else
            LogLevel levelDefault = LogLevel.Error;
#endif

            try
            {
                // ищем рядом с нашей dll
                string levelFile = Path.Combine(LoaderEnvironment.AssemblyDirectory, "loglevel.txt");

                if (!File.Exists(levelFile)) return levelDefault;

                //foreach (string line in File.ReadLines(levelFile))
                //{
                //    string text = line.Trim();

                //    if (string.IsNullOrWhiteSpace(text))
                //    {
                //        continue;
                //    }

                //    LogLevel level = LogLevel.FromString(text);

                //    return level ?? LevelDefault;
                //}

                //return LevelDefault;
                // Читаем без блокировки файла
                using FileStream fs = new FileStream(levelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader sr = new StreamReader(fs);
                string? text = sr.ReadLine()?.Trim();

                return string.IsNullOrWhiteSpace(text) ? levelDefault : (LogLevel.FromString(text) ?? levelDefault);

            }
            catch
            {
                return levelDefault;
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