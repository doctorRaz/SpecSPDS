using dRz.Loader.Cad.Infrastructure;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>
/// 
/// </summary>
public static class LogBootstrapAsync
{

    /// <summary>
    /// The initialized
    /// </summary>
    private static bool _initialized;


    /// <summary>
    /// The synchronize
    /// </summary>
    private static readonly object _sync = new();


    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized)
            return;

        lock (_sync)
        {
            if (_initialized)
                return;

            ConfigureInternalLogger();

            SetupGlobalContext();

            LoadConfiguration();

            _initialized = true;

        }
    }

    /// <summary>
    /// Setups the global context.
    /// </summary>
    private static void SetupGlobalContext()
    {
#if DEBUG
        // фиксируем момент старта процесса
        GlobalDiagnosticsContext.Set(
            "DateCreate",
            DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture)
        );
#endif

        GlobalDiagnosticsContext.Set("LogsDir", LoaderEnvironment.AppDataProductLogPath);
        GlobalDiagnosticsContext.Set("AppName", LoaderEnvironment.ProductName);
    }


    /// <summary>
    /// Loads the configuration.
    /// </summary>
    private static void LoadConfiguration()
    {

        LogManager.Configuration = null;

        LoggingConfiguration config = new LoggingConfiguration();
        LogLevel level = ReadLogLevelOnce();

        FileTarget fileTarget = new FileTarget("xmlFile")
        {
            FileName = Path.Combine(appDirLogs, $"${{shortdate}}_{LoaderEnvironment.ProductTitle}.log"),
            //FileName = Path.Combine(appDirLogs, $"${{date:universalTime=true:format=yyyy-MM-dd HH}}_{LoaderEnvironment.ProductName}.log"),

            ArchiveFileName = Path.Combine(appDirLogs, $"${{shortdate}}_{LoaderEnvironment.ProductTitle}.{{#}}.log"),
            //ArchiveFileName = Path.Combine(appDirLogs, $"${{date:universalTime=true:format=yyyy-MM-dd HH}}_{LoaderEnvironment.ProductName}.{{#}}.log"),


            ArchiveEvery = FileArchivePeriod.Day,
            ArchiveAboveSize = 5 * 1024  * 1024,
            MaxArchiveFiles = 10,
          
            KeepFileOpen = false,
            Layout = CreateXmlLayout()
        };

        // ---------------------------
        // Async wrapper
        // ---------------------------
        var asyncTarget = new AsyncTargetWrapper(fileTarget)
        {
            QueueLimit = 10000,              // размер очереди
            OverflowAction = AsyncTargetWrapperOverflowAction.Discard,
            BatchSize = 100,
            TimeToSleepBetweenBatches = 50
        };

        config.AddTarget("asyncFile", asyncTarget);
        config.LoggingRules.Add(new LoggingRule("*", level, asyncTarget));
        //config.LoggingRules.Add(new LoggingRule("*", level, fileTarget));

        LogManager.Configuration = config;

    }

    /// <summary>
    /// Internal logger → Output Window (DEBUG)
    /// </summary>
    private static void ConfigureInternalLogger()
    {
#if DEBUG

        InternalLogger.LogLevel = LogLevel.Info;

        //пишем в файл
        InternalLogger.LogFile = LogFile();

        InternalLogger.LogWriter = new OutputDebugStringWriter();

        InternalLogger.LogToConsole = false;

        //все исключения
        LogManager.ThrowExceptions = false;

        //ошибки конфига
        LogManager.ThrowConfigExceptions = true;

        InternalLogger.Info($"{LoaderEnvironment.FileName}: InternalLogger Initialize manual");

#else

        InternalLogger.LogLevel = LogLevel.Off;

#endif
    }

    /// <summary>
    /// Logs the file.
    /// </summary>
    /// <returns></returns>
    static string LogFile()
    {

        string logTimestamp = $"{DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";


        return Path.Combine(appDirLogs, $"{logTimestamp}_{LoaderEnvironment.FileName}_internal.log");

    }

    /// <summary>
    /// The application dir logs
    /// </summary>
    private readonly static string appDirLogs = LoaderEnvironment.AppDataProductLogPath;

    /// <summary>
    /// отладочная информация из nLog в output VS только для отладки!!!
    /// </summary>
    /// <seealso cref="TextWriter" />
    private sealed class OutputDebugStringWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string? value)
        {
            Debug.WriteLine("[NLog] " + value);
        }

        public override void Write(string? value)
        {
            Debug.Write("[NLog] " + value);
        }
    }

    /// <summary>
    /// Чтение уровня лога один раз
    /// </summary>
    /// <returns>LogLevel</returns>
    private static LogLevel ReadLogLevelOnce()
    {
#if DEBUG
        LogLevel LevelDefault = LogLevel.Trace;
#else
        LogLevel LevelDefault=LogLevel.Error;
#endif

        try
        {
            // ищем рядом с dll
            string AppDir = LoaderEnvironment.AssemblyDirectory;

            string levelFile = Path.Combine(AppDir, "loglevel.txt");

            if (!File.Exists(levelFile))
            {
                return LevelDefault;
            }


            foreach (var line in File.ReadLines(levelFile))
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
            MaxRecursionLimit=10,
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
}
