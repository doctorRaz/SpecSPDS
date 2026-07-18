//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace dRz.Test.OpenDwg
//{
//    internal class ConfigNLog
//    {
//        static void ConfigureNLog()
//        {
//            string date = DateTime.Now.ToString("yyyyMMdd-HH_mm_ss", CultureInfo.InvariantCulture);

//            string appName = Services.CallerName(count);

//            string name = "${callsite:className=false:methodName=true:includeSourcePath=false}";

//            string fileName = $"${{basedir}}/logs/{date}_${{callsite:className=false:methodName=true:includeSourcePath=false}}_{appName}";

//            LoggingConfiguration config = new LoggingConfiguration();

//            // Target для отдельного класса
//            FileTarget fileTarget = new FileTarget
//            {
//                //Name = $"{caller}",
//                Name = name,// "${callsite:className=false:methodName=true:includeSourcePath=false}",
//                //FileName = "${date:format=yyyyMMdd-HH_mm_ss}_${callsite:className=false:methodName=true:includeSourcePath=false}.log",
//                FileName = $"{fileName}.log",// $"${{basedir}}/logs/{date}_${{callsite:className=false:methodName=true:includeSourcePath=false}}.log",
//                //Layout = "${longdate} | " +
//                //         "${level:uppercase=true} | " +
//                //         "${callsite:className=true:methodName=true:includeSourcePath=false} | " +
//                //         "Line:${callsite-linenumber} | " +
//                //         "${message} | " +
//                //         "${exception:format=ToString}"
//            };

//            FileTarget fileTargetErr = new FileTarget
//            {
//                //Name = $"{caller}",
//                Name = $"{name}_err",// "${callsite:className=false:methodName=true:includeSourcePath=false}_Err",
//                //FileName = "${date:format=yyyyMMdd-HH_mm_ss}_${callsite:className=false:methodName=true:includeSourcePath=false}_Err.log",
//                FileName = $"{fileName}_Err.log",// $"${{basedir}}/logs/${date}_${{callsite:className=false:methodName=true:includeSourcePath=false}}_Err.log",
//                //Layout = "${longdate} | ${level:uppercase=true} | " +
//                //         "${callsite:methodName=true} | " +
//                //         "User[${windows-identity}] | " +
//                //         "Session:${aspnet-sessionid} | " +
//                //         "Request:${aspnet-request-url} | " +
//                //         "${message} | " +
//                //         "${when:when=length('${exception}')>0:Inner=${newline}StackTrace: ${exception:format=StackTrace}}"
//            };


//            FileTarget fileTargetResult = new FileTarget
//            {
//                Name = $"{name}_result",

//                FileName = "${basedir}/logs/${shortdate}_result.log", // $"{fileName}_result.log",

//            };

//            // Оборачиваем таргеты в асинхронные обертки
//            AsyncTargetWrapper asyncFileTarget = new AsyncTargetWrapper
//            {
//                Name = "async_" + name,
//                WrappedTarget = fileTarget,
//                QueueLimit = 10000,                     // Максимальный размер очереди
//                OverflowAction = AsyncTargetWrapperOverflowAction.Discard, // Действие при переполнении
//                TimeToSleepBetweenBatches = 50,         // Пауза между пакетами (мс)
//                BatchSize = 100                         // Размер пакета для записи
//            };

//            AsyncTargetWrapper asyncFileTargetErr = new AsyncTargetWrapper
//            {
//                Name = "async_" + name + "_Err",
//                WrappedTarget = fileTargetErr,
//                QueueLimit = 10000,
//                OverflowAction = AsyncTargetWrapperOverflowAction.Discard,
//                TimeToSleepBetweenBatches = 50,
//                BatchSize = 100
//            };


//            AsyncTargetWrapper asyncFileTargetResult = new AsyncTargetWrapper
//            {
//                Name = "async_" + name + "_result",
//                WrappedTarget = fileTargetResult,
//                QueueLimit = 10000,
//                OverflowAction = AsyncTargetWrapperOverflowAction.Discard,
//                TimeToSleepBetweenBatches = 50,
//                BatchSize = 100
//            };



//            config.AddRule(LogLevel.Trace, LogLevel.Warn, fileTarget);
//            config.AddRule(LogLevel.Error, LogLevel.Fatal, fileTargetErr);
//            config.AddRuleForOneLevel(LogLevel.Warn, fileTargetResult);

//            //config.AddRule(LogLevel.Trace, LogLevel.Info, asyncFileTarget);
//            //config.AddRule(LogLevel.Error, LogLevel.Fatal, asyncFileTargetErr);
//            //config.AddRuleForOneLevel(LogLevel.Warn, asyncFileTargetResult);

//            //config.AddTarget(fileTarget);

//            //// Правило только для конкретного класса
//            //config.AddRuleForOneLevel(LogLevel.Trace, fileTarget, "MyNamespace.MyClass");

//            LogManager.Configuration = config;
//        }

//    }
//}
