using dRz.Loader.Cad.Infrastructure;
using dRz.Loader.Cad.Infrastructure.InternalDiagnostic;
using NLog;
using System;
using System.Globalization;
using System.IO;


//todo только для отладки загрузки nlog конфигурации
//скопировать в dRz.Cad.Cad.Bootstrap
// скопировать в dRz.SpecSPDS.Cad.Bootstrap

namespace dRz.Loader.Cad.Infrastructure.Bootstrap
{
    //todo добавить установку пути в %appdata%\product\logs
    /// <summary>
    /// 
    /// </summary>
    public class LogBootstrap
    {
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