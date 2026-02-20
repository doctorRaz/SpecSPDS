using dRz.Loader.nCad.Infrastructure;
using NLog;
using System;
using System.Globalization;

namespace dRz.Loader.nCad.Infrastructure.Bootstrap;

internal static class SetupGlobalContextHelpers
{

    /// <summary>
    /// Setups the global context.
    /// </summary>
    internal static void SetupGlobalContext()
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
}