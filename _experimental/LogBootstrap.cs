using NLog;
using System.IO;


//todo только для отладки загрузки nlog конфигурации
//скопировать в dRz.Cad.Cad.Bootstrap
// скопировать в dRz.SpecSPDS.Cad.Bootstrap

namespace dRz.Experimental.Bootstrap
{
    public static class LogBootstrap
    {
        public static void Init()
        {
            NLog.Config.LoggingConfiguration? config = LogManager.Configuration;//is Config?
            #region load  nlogLoader.config

            string? dllDir = Path.GetDirectoryName(typeof(LogBootstrap).Assembly.Location);

            string configPath = Path.Combine(dllDir, "NLogTest.config");

            LogManager.Setup().LoadConfigurationFromFile(configPath);

            //todo добавить установку пути в %appdata%\product\logs

            //todo посмотреть что там загрузилось
              config = LogManager.Configuration;

            #endregion

        }

    }
}
