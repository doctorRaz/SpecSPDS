using NLog;
using dRz.SpecSPDS.Core.InternalDiagnostic;
using System;
using dRz.SpecSPDS.Core.Bootstrap;
using System.Diagnostics;
using System.ComponentModel;


#if AC

using Rtm = Autodesk.AutoCAD.Runtime;

#elif NC

using App = HostMgd.ApplicationServices.Application;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using Rtm = Teigha.Runtime;
#endif

[assembly: Rtm.ExtensionApplication(typeof(dRz.SpecSPDS.nCad.EntryPoint))]

namespace dRz.SpecSPDS.nCad
{
    public /*partial*/ class EntryPoint : Rtm.IExtensionApplication
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

#if DEBUG
        [Rtm.CommandMethod("инитАд")]
        [Description("ручной инит адаптера")]
#endif
        public void Initialize()
        {
            //если нет библиотек или еще какой косяк
            try
            {

                AssemblyResolver assembLyResolve = new AssemblyResolver();
                assembLyResolve.Register();//add  event Assembly resolve  

                InitLoger();

                InitAdapter();
            }
            catch (Exception ex)
            {
                Document doc = App.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    return;
                }

                Editor ed = doc.Editor;

                ed.WriteMessage($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        private void InitLoger()
        {
            try
            {

#if DEBUG
                //разные фабрики
                Debug.WriteLine(LogManager.LogFactory.GetHashCode());
                //чисто для диагностики ручное включение
                new InternalLoggerDiagnostic("Internal logger manual DEBUG");
#endif

                var conf = LogManager.Configuration;

                //если лог конфиг не загрузился сам грузим руками
                if (LogManager.Configuration is null)
                {
                    //пытаемся грузить принудительно
                    new LogBootstrap();

                    //если конфиг не нашелся и не загрузился
                    if (LogManager.Configuration is null)
                    {
                        //включим диагностику eсли выключена
                        new InternalLoggerDiagnostic("LogManager empty Configuration");

                        //дальше пишем внутренний лог
                    }
                }

                log.Info("Logger Started");

            }
            catch (Exception ex)
            {
                Document doc = App.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    return;
                }

                Editor ed = doc.Editor;

                ed.WriteMessage($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        private void InitAdapter()
        {
            Loader.HelloSpec();
        }

        public void Terminate()
        {
            log.Info("LogManager.Shutdown");

            LogManager.Shutdown();
        }
    }

    class Loader
    {
        internal static void HelloSpec()
        {
            Document doc = App.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }

            Editor ed = doc.Editor;

            ed.WriteMessage($"Hello SpecSPDS for nanoCAD {App.Version.Major.ToString()}.{App.Version.Minor.ToString()}");
        }
    }



}