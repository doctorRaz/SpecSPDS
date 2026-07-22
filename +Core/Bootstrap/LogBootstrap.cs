using NLog;
using System.IO;

//todo только для отладки загрузки nlog конфигурации
//скопировать в dRz.Cad.Cad.Bootstrap
// скопировать в dRz.SpecSPDS.Cad.Bootstrap

namespace dRz.Core.Bootstrap
{
    //todo добавить установку пути в %appdata%\product\logs
    /// <summary>
    ///
    /// </summary>
    public class LogBootstrap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogBootstrap"/> class.
        /// </summary>
        public LogBootstrap()
        {
            LogManager.Setup().LoadConfigurationFromFile(configPath());
        }

        private string configPath()
        {
            //путь получать из сборки
            string dllDir = Path.GetDirectoryName(typeof(LogBootstrap).Assembly.Location);

            return Path.Combine(dllDir, nLogConfigFileName);
        }

        private const string nLogConfigFileName = "NLog.dll.test.nlog";
    }
}