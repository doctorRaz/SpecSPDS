#if DEBUG

using System.ComponentModel;
using System.Diagnostics;
using NLog;
using System.Globalization;
using NLog.Config;
using NLog.Common;









#if NC || NC26
using Teigha.Runtime;

#elif AC
using Db = Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.Runtime;
using App = Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

#endif


namespace dRz.SpecSPDS.Cad.Commands.Test
{
    public class NlogTest
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        static int count = 10;

        /// <summary>
        /// проверка работы лога
        /// </summary>
        [CommandMethod("лог")]
        [Description("проверка работы лога")]
        public static void TestLog()
        {

            //подумать куда удобнее писать логи???
            //  addonDir/logs - рядом с адоном
            //      плюс, не сорим больше нигде,
            //      минус юзер не помнит куда положил аддон
            //      
            //  %appdata%/addonName/logs
            //      плюс - всегда можно сказать юзеру где брать логи, заодно там будет %appdata%/addonName/settings,
            //      можно забрать  все одним зипом и не искать по всему диску
            //      минус? - надо подумать

            // как получать имя аддона??
            //  1. asembly?
            //  2. задавать в коде
            //  3. ??



            //по хорошему этот метод вызывать из инициализации аддона
            //заодно можно писать в sys.log диагностическую инфу о системе, что б меньше вопросов юзерам задавать
            LogBootstrap.Init();//грузим nlog.config принудительно
                                //можно сюда вынести управление диагностикой логера
                                //и установку переменных в конфиге





            //GlobalDiagnosticsContext.Set("appName", ServicesCAD.CallerName(count));

            string logTimestamp = $"{DateTime.Now.ToString("yyyyMMdd-HH_mm_ss", CultureInfo.InvariantCulture)}_";

            GlobalDiagnosticsContext.Set("logTimestamp", logTimestamp);

            var config = LogManager.Configuration;

            log.Info("Performance metrics: " +
        "Memory: {MemoryUsage}MB, " +
        /*   "CPU: {CpuUsage}%, " +*/
        "Threads: {ThreadCount}, " +
        "Handles: {HandleCount}",
        Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024,
        /*GetCpuUsage(),*/
        Process.GetCurrentProcess().Threads.Count,
        Process.GetCurrentProcess().HandleCount);


            try
            {
                log.ForInfoEvent()
                   .Message("Начало работы")
                   .Property("userId", "wwweew")
                   .Property("property1", 123)
                   .Log();


                log.Trace($"************  1 *************");
                log.Info("This is a message from {User}", "Mickey Donovan");

                var msg = new LogEventInfo(LogLevel.Info, "", "This is a message");
                msg.Properties.Add("User", "Ray Donovan");
                log.Info(msg);

                log.Info(string.Format("This is a message from {0}", "Mickey Donovan"));

                log.Trace($"Trace");
                log.Debug($"Debug");
                log.Info($"Info");
                log.Warn($"Warn");
                log.Error($"Error");
                log.Fatal($"Fatal");

                log.Error(new System.Exception(), "This is an error message");




                int e = 0;

                int ii = 10 / e;
            }
            catch (System.Exception ex)
            {
                log.ForErrorEvent()
                   .Exception(ex)
                   .Property("userId", 50000)
                   .Property("property1", 123)
                   .Log();

                log.Info("Продолжение работы после ошибки");

                log.Error(ex);

            }
            finally
            {
                var MemoryUsage = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;
                var ThreadCount = Process.GetCurrentProcess().Threads.Count;


                log.ForInfoEvent()
                                   .Message("Performance metrics")
                                   .Property("userId", $"Memory: {MemoryUsage}MB")
                                   .Property("property1", $"Threads: {ThreadCount}")
                                   .Log();

                LogManager.Shutdown();
            }





        }

    }

    public static class LogBootstrap
    {
        public static void InternalLoggerOn_OFF(bool enabled = false)
        {
            #region диагностика nLog

            InternalLogger.LogLevel = LogLevel.Trace;

            InternalLogger.LogToConsole = enabled;

            #endregion
        }

        public static void Init()
        {

            #region load  nlog.config

            string? dllDir = Path.GetDirectoryName(
                typeof(LogBootstrap).Assembly.Location);

            string configPath = Path.Combine(dllDir, "nlog.config");

            LogManager.Configuration = new XmlLoggingConfiguration(configPath);

            #endregion

            InternalLogger.LogFile = Path.Combine(dllDir, "${shortdate}_nlog-internal.log");

        }

        //GPT
        public static void LogProcessMetrics(ILogger log)
        {
            var p = Process.GetCurrentProcess();

            log.ForInfoEvent()
               .Message("Process performance metrics")
               .Property("memoryMb", Math.Round(p.WorkingSet64 / 1024d / 1024d, 2))
               .Property("threadCount", p.Threads.Count)
               .Property("pid", p.Id)
               .Property("processName", p.ProcessName)
               .Log();
        }
    }
}


#endif