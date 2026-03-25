using dRz.CAD.Runtime.Info;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.IO;

namespace dRz.Loader.Infrastructure.Logging
{
    public static class NlogFactory
    {
        private static readonly LogFactory Factory;
        //public static readonly Logger Logger;

        static NlogFactory()
        {
            Factory = new LogFactory();

            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget("file")
            {
                FileName = Path.Combine(InfoAdOn.AppDataProductLogPath, InfoAdOn.FileName + ".log"),

                Layout = "${longdate}|${level}|${logger}|${message}|${exception}"
            };

            config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);

            Factory.Configuration = config;

            //Logger = Factory.GetLogger(InfoAdOn.ProductName);
        }

        public static Logger GetLogger<T>()
        {
            string loggerName = $"{InfoAdOn.ProductName}.{typeof(T).FullName}";
            return Factory.GetLogger(loggerName);
        }


    }
}
