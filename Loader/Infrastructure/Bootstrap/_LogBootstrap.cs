using System;
using System.Globalization;
using NLog;
using NLog.Common;

namespace dRz.Loader.Cad.Infrastructure.Bootstrap
{
    // https://chatgpt.com/c/698b67b7-61e0-838c-99b6-4c3785949f94#:~:text=%D0%A3%D0%BB%D1%83%D1%87%D1%88%D0%B5%D0%BD%D0%BD%D0%B0%D1%8F%20%D1%80%D0%B5%D0%B0%D0%BB%D0%B8%D0%B7%D0%B0%D1%86%D0%B8%D1%8F

    public static class _LogBootstrap
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

                SetupGlobalContext();
                LoadConfiguration();

                _initialized = true;
            }
        }

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

        private static void LoadConfiguration()
        {
            string configPath = LoaderEnvironment.NLogConfigPath;

            try
            {
                LogManager.Setup()
                          .LoadConfigurationFromFile(configPath);

                if (LogManager.Configuration == null)
                {
                    EnableInternalDiagnostics(
                        $"NLog configuration is null. Path: {configPath}"
                    );
                }
            }
            catch (Exception ex)
            {
                EnableInternalDiagnostics(
                    $"Failed to load NLog configuration. Path: {configPath}",
                    ex
                );
            }
        }

        private static void EnableInternalDiagnostics(string message, Exception? ex = null)
        {
            // Включаем внутреннюю диагностику принудительно
            InternalLogger.LogLevel = LogLevel.Trace;
            InternalLogger.LogToConsole = true;
            InternalLogger.LogFile = System.IO.Path.Combine(
                LoaderEnvironment.AppDataProductLogPath,
                "nlog-internal.log"
            );

            if (ex != null)
            {
                InternalLogger.Error(message, ex);
            }
            else
            {
                InternalLogger.Error(message);

            }
        }
    }
}