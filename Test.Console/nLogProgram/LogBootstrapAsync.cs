using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

public static class LogBootstrapAsync
{
    private static bool _initialized;

    private static readonly object _sync = new();

    public static void Initialize()
    {
        if (_initialized)
            return;

        lock (_sync)
        {
            if (_initialized)
                return;

            ConfigureInternalLogger();

            LogManager.Configuration = null;

            var config = new LoggingConfiguration();
            var level = ReadLogLevelOnce();

            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MyProduct",
                "Logs");

            //Directory.CreateDirectory(logDir);

            var fileTarget = new FileTarget("xmlFile")
            {
                FileName = Path.Combine(logDir, "application.log"),

                ArchiveFileName = Path.Combine(logDir, "application.{#}.log"),
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveAboveSize = 5 * 1024 * 1024,
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

            LogManager.Configuration = config;

            _initialized = true;
        }
    }

    // --------------------------------------------------
    // Internal logger → Output Window (DEBUG)
    // --------------------------------------------------
    private static void ConfigureInternalLogger()
    {
#if DEBUG
        InternalLogger.LogLevel = LogLevel.Trace;
        InternalLogger.LogToConsole = false;
        InternalLogger.LogWriter = new OutputDebugStringWriter();
#else
        InternalLogger.LogLevel = LogLevel.Off;
#endif
    }

    private sealed class OutputDebugStringWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string value)
        {
            Debug.WriteLine("[NLog] " + value);
        }

        public override void Write(string value)
        {
            Debug.Write("[NLog] " + value);
        }
    }

    // --------------------------------------------------
    // Чтение уровня один раз
    // --------------------------------------------------
    private static LogLevel ReadLogLevelOnce()
    {
        try
        {
            var levelFile = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "loglevel.txt");

            if (!File.Exists(levelFile))
                return LogLevel.Error;

            foreach (var line in File.ReadLines(levelFile))
            {
                var text = line.Trim();
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                var level = LogLevel.FromString(text);
                return level ?? LogLevel.Error;
            }

            return LogLevel.Error;
        }
        catch
        {
            return LogLevel.Error;
        }
    }

    // --------------------------------------------------
    // XML layout
    // --------------------------------------------------
    private static XmlLayout CreateXmlLayout()
    {
        return new XmlLayout
        {
            IncludeEventProperties = true,
            IndentXml = true,
            ElementName = "logevent",

            Attributes =
            {
                new XmlAttribute("time", "${longdate}"),
                new XmlAttribute("level", "${level:uppercase=true}"),
                new XmlAttribute("logger", "${logger}"),
                new XmlAttribute("thread", "${threadid}")
            },

            Elements =
            {
                new XmlElement("message", "${message}"),
                new XmlElement("exception", "${exception:format=ToString}")
            }
        };
    }
}
