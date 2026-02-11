using System.ComponentModel;
using NLog;
using dRz.SpecSPDS.Core.InternalDiagnostic;
using System;
using dRz.SpecSPDS.Core.Bootstrap;




#if AC

using Rtm = Autodesk.AutoCAD.Runtime;

#elif NC

using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using Rtm = Teigha.Runtime;
#endif

[assembly: Rtm.ExtensionApplication(typeof(dRz.SpecSPDS.Cad.EntryPoint))]

namespace dRz.SpecSPDS.Cad
{
    public class EntryPoint : Rtm.IExtensionApplication
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

#if DEBUG
        [Rtm.CommandMethod("инитАд")]
        [Description("ручной инит адаптера")]
#endif
        public void Initialize()
        {
            //если нет библиотек или еще какой косяк
            try
            {
                Init();
            }
            catch (Exception ex)
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    return;
                }

                Editor ed = doc.Editor;

                ed.WriteMessage($"{ex.Message}\n{ex.StackTrace}");
            }
        }

        private void Init()
        {
            try
            {

#if DEBUG
                //чисто для диагностики ручное включение
                new InternalLoggerDiagnostic("Internal logger manual DEBUG");
#endif

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

                Loader.HelloSpec();

            }
            catch (Exception ex)
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    return;
                }

                Editor ed = doc.Editor;

                ed.WriteMessage($"{ex.Message}\n{ex.StackTrace}");
            }
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
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }

            Editor ed = doc.Editor;

            ed.WriteMessage($"Hello SpecSPDS for nanoCAD 23-26");
        }
    }



}