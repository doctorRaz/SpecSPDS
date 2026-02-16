using dRz.Loader.Cad.Infrastructure;
using dRz.Loader.Cad.Infrastructure.InternalDiagnostic;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using System;
using System.IO;

/// <summary>
/// один таргет только для загрузчика
/// </summary>
public static class LogBootstrapLoader
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

#if DEBUG
            EnableInternalDiagnostics();
#endif

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
    }

    /// <summary>
    /// Enables the internal diagnostics.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="ex">The ex.</param>
    private static void EnableInternalDiagnostics(string message = "", Exception? ex = null)
    {

        // --- Internal log ---
        InternalLogger.LogWriter = new DebugTextWriter();//output VS
        InternalLogger.LogLevel = LogLevel.Warn;
        InternalLogger.LogFile = Path.Combine(LoaderEnvironment.AppDataProductLogPath,"internal.log");
        InternalLogger.LogToConsole = true;
        LogManager.ThrowExceptions = false;
        LogManager.ThrowConfigExceptions = true;

    }

    /// <summary>
    /// Loads the configuration.
    /// </summary>
    private static void LoadConfiguration()
    {


        var config = new LoggingConfiguration();

        // =========================
        // Variables
        // =========================
        config.Variables["LevelMay"] = "Trace";
        config.Variables["maxArchiveFiles"] = "10";
        config.Variables["archiveAboveSize"] = "5242880";

        // =========================
        // Targets (async=true)
        // =========================

        // ---------- XML Loader ----------
        var xmlFileLoader = new FileTarget("xmlFileLoader")
        {
            FileName = "${gdc:LogsDir}/${date:universalTime=true:format=yyyy-MM-dd HH}_${gdc:AppName}_Loader.log",
            ArchiveEvery = FileArchivePeriod.Day,
            ArchiveAboveSize = 5242880,
            ArchiveNumbering = ArchiveNumberingMode.Rolling,
            ArchiveFileName = "${gdc:LogsDir}/${date:universalTime=true:format=yyyy-MM-dd HH}_${gdc:AppName}_Loader.{#}.log",
            MaxArchiveFiles = 10,
            KeepFileOpen = false,
            Layout = new XmlLayout
            {
                IncludeEventProperties = true,
                IndentXml = true,
                MaxRecursionLimit = 10,
                ElementName = "logevent",
                Attributes =
                {
                    new XmlAttribute("time", "${longdate}"),
                    new XmlAttribute("level", "${level:upperCase=true}"),
                    new XmlAttribute("logger", "${logger}")
                },
                Elements =
                {
                    new XmlElement("message", "${message}"),
                    new XmlElement("error", "${exception:format=ToString}")
                }
            }
        };

        // ---------- XML Adapter ----------
        var xmlFileAdapter = new FileTarget("xmlFileAdapter")
        {
            FileName = "${gdc:LogsDir}/${date:universalTime=true:format=yyyy-MM-dd HH}_${gdc:AppName}_Adapter.log",
            ArchiveEvery = FileArchivePeriod.Day,
            ArchiveAboveSize = 5242880,
            ArchiveNumbering = ArchiveNumberingMode.Rolling,
            ArchiveFileName = "${gdc:LogsDir}/${date:universalTime=true:format=yyyy-MM-dd HH}_${gdc:AppName}_Adapter.{#}.log",
            MaxArchiveFiles = 10,
            KeepFileOpen = false,
            Layout = xmlFileLoader.Layout
        };

        // ---------- CSV All ----------
        var csvFileAll = new FileTarget("csvFileAll")
        {
            FileName = "${gdc:LogsDir}/${date:universalTime=true:format=yyyy-MM-dd HH}_${gdc:AppName}_All.log",
            Layout = new CsvLayout
            {
                Delimiter = CsvColumnDelimiterMode /*CsvColumnDelimiter*/.Tab,
                WithHeader = true,
                Quoting = CsvQuotingMode.All,
                Columns =
                {
                    new CsvColumn("time", "${longdate}"),
                    new CsvColumn("level", "${level:upperCase=true}"),
                    new CsvColumn("logger", "${logger}"),
                    new CsvColumn("message", "${message}"),
                    new CsvColumn("exception", "${exception:format=ToString}"),
                    new CsvColumn("prop1", "${event-properties:prop1}"),
                    new CsvColumn("prop2", "${event-properties:prop2}")
                }
            }
        };

        // ---------- Debug logfile ----------
        var logfile = new FileTarget("logfile")
        {
            FileName = "${nlogdir:dir=logs}/${gdc:DateCreate}_${gdc:Caller}_${processname}.log"
        };

        // Оборачиваем в Async
        config.AddTarget(new AsyncTargetWrapper(xmlFileLoader));
        config.AddTarget(new AsyncTargetWrapper(xmlFileAdapter));
        config.AddTarget(new AsyncTargetWrapper(csvFileAll));
        config.AddTarget(new AsyncTargetWrapper(logfile));

        // =========================
        // Rules
        // =========================

        var minLevel = LogLevel.FromString(config.Variables["LevelMay"].ToString());

        config.LoggingRules.Add(new LoggingRule("*", minLevel, logfile));
        config.LoggingRules.Add(new LoggingRule("*", minLevel, csvFileAll));

        var loaderRule = new LoggingRule("dRz.Loader*", minLevel, xmlFileLoader)
        {
            Final = true
        };
        config.LoggingRules.Add(loaderRule);

        config.LoggingRules.Add(new LoggingRule("*", minLevel, xmlFileAdapter));

        LogManager.Configuration = config;

        _initialized = true;
    }
}
