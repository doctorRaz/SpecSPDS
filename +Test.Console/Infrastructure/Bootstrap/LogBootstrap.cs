using dRz.Loader.nCad.Infrastructure;
using dRz.SpecSpds.Test.Infrastructure.InternalDiagnostic;
using NLog;
using System;
using System.Globalization;


//todo только для отладки загрузки nlog конфигурации
//скопировать в dRz.nCad.nCad.Bootstrap
// скопировать в dRz.SpecSPDS.nCad.Bootstrap

namespace dRz.SpecSpds.Test.Infrastructure.Bootstrap
{
    //todo добавить установку пути в %appdata%\product\logs
    /// <summary>
    /// 
    /// </summary>
    public /*static*/ class LogBootstrap
    {

        private static bool _initialized;
        private static readonly object _sync = new();

        public static void Initialize()
        {



        }

        /// <summary>
        /// настройка, конфигурирование логгера
        /// </summary>
        public LogBootstrap()
        {
#if DEBUG
            string logTimestamp = $"{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture)}";

            //дата лога до секунды, во время сеанса не меняем
            GlobalDiagnosticsContext.Set("DateCreate", logTimestamp);

#endif
            //установка каталога логов
            GlobalDiagnosticsContext.Set("LogsDir", LoaderEnvironment.AppDataProductLogPath);
            //имя продукта (часть имени файла лога)
            GlobalDiagnosticsContext.Set("AppName", LoaderEnvironment.ProductName);

            string configPath = LoaderEnvironment.NLogConfigPath;

            LogManager.Setup().LoadConfigurationFromFile(configPath);

            //если конфиг не завелся
            if (LogManager.Configuration is null)
            {
                //включим диагностику eсли выключена
                new InternalLoggerDiagnostic("LogManager empty Configuration");

                //дальше пишем во внутренний лог
            }
        }

    }
}