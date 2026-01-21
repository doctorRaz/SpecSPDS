using NLog;
using System.IO;


//todo только для отладки загрузки nlog конфигурации
//скопировать в dRz.nCad.Loader.Infrastructure
// скопировать в dRz.SpecSPDS.Cad.Bootstrap

namespace dRz.Experimental.Bootstrap
{
    public static class LogBootstrap
    {
        public static void Init()
        {

            #region load  nlogLoader.config

            string? dllDir = Path.GetDirectoryName(typeof(LogBootstrap).Assembly.Location);

            string configPath = Path.Combine(dllDir, "NLogTest.config");

            LogManager.Setup().LoadConfigurationFromFile(configPath);

            //посмотреть что там загрузилось
            NLog.Config.LoggingConfiguration? config = LogManager.Configuration;

            #endregion

        }

    }
}
