using NLog;
using NLog.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dRz.nCad.Loader.Infrastructure
{
    internal static class LogBootstrap
    {
        public static void Init()
        {

            #region load  nlog.config

            string? dllDir = Path.GetDirectoryName(
                typeof(LogBootstrap).Assembly.Location);

            string configPath = Path.Combine(dllDir, "nlog.config");

            LogManager.Setup().LoadConfigurationFromFile(configPath);
            //LogManager.Configuration = new XmlLoggingConfiguration(configPath);

            #endregion

            //InternalLoggerOn_OFF(true);

            string logTimestamp = $"{DateTime.Now.ToString("yyyyMMdd-HH_mm_ss", CultureInfo.InvariantCulture)}_";

            GlobalDiagnosticsContext.Set("logTimestamp", logTimestamp);

            var config = LogManager.Configuration;

            InternalLogger.LogFile = Path.Combine(dllDir, "logs", $"{logTimestamp}_nlog-internal.log");

        }
             
    }
}
