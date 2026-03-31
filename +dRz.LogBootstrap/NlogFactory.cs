using NLog;
using static dRz.Loader.Infrastructure.AddonContext;

namespace dRz.Loader.Infrastructure.Logging
{
    public static class NlogFactory
    {
        private static readonly LogFactory Factory;
        //public static readonly Logger Logger;

        static NlogFactory()
        {
            Factory = new LogFactory();

            //x прибить, конфиг из метода
            /*
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget("file")
            {
                FileName = Path.Combine(InfoAdOn.AppDataProductLogPath, InfoAdOn.FileName + ".log"),

                KeepFileOpen = false,

                Layout = "${longdate}|${level}|${logger}|${processid}|${message}|${exception}"
            };

            config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);
            */
            //todo грузить конфиг из файла
            // если нет, то программно,
            // сделать как в глобальном логере
            Factory.Configuration = LogBootstrap.CreateConfiguration();// config;

            //todo фабрику в контейнер
            //Logger = Factory.GetLogger(InfoAdOn.ProductName);
        }

        public static Logger GetLogger<T>()
        {
            //string loggerName = $"{InfoDll.ProductName}.{typeof(T).FullName}";
            string loggerName = $"{typeof(T).FullName}";
            return Factory.GetLogger(loggerName);
        }


    }
}
