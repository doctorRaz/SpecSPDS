using NLog;
using NLog.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//todo только для отладки загрузки nlog конфигурации
//скопировать в dRz.nCad.Loader.Infrastructure
// скопировать в dRz.SpecSPDS.Cad.Bootstrap

namespace dRz.Experimental.Bootstrap
{
    public static class LogBootstrapper
    {
        public static void Init()
        {

            #region load  nlogLoader.config

            string? dllDir = Path.GetDirectoryName(typeof(LogBootstrapper).Assembly.Location);

            string configPath = Path.Combine(dllDir, "NLog.config");

            LogManager.Setup().LoadConfigurationFromFile(configPath);

            //посмотреть что там загрузилось
            NLog.Config.LoggingConfiguration? config = LogManager.Configuration;

            #endregion
            
        }

    }
}
