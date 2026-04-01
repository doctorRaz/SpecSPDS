using dRz.Log.Interfaces;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Concurrent;
using System.IO;

public sealed class LogService : ILogService
{
    private static readonly ConcurrentDictionary<string, LogFactory> _factories = new();

    private readonly Func<string> _productNameProvider;
    private readonly Func<string?> _configPathProvider;

    public LogService(
        Func<string> productNameProvider,
        Func<string?> configPathProvider)
    {
        _productNameProvider = productNameProvider ?? throw new ArgumentNullException(nameof(productNameProvider));
        _configPathProvider = configPathProvider ?? (() => null);
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
        LogFactory factory = new LogFactory();

        try
        {
            string configPath = _configPathProvider();

            if (!string.IsNullOrWhiteSpace(configPath) && File.Exists(configPath))//todo тут проверка конфига на нулл, конфиг если есть подгружается сам
            {
                factory.Configuration = new XmlLoggingConfiguration(configPath);
                return factory;
            }
        }
        catch
        {
            // Игнорируем и идём в fallback
        }

        factory.Configuration = CreateFallbackConfig(productName);
        return factory;
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
}