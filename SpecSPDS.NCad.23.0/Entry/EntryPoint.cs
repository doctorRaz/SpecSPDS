using System.ComponentModel;
using System.Diagnostics;
using NLog;
using dRz.SpecSPDS.Cad.Application;
using dRz.SpecSPDS.Core.InternalDiagnostic;
using dRz.Experimental.Bootstrap;






#if AC


using Rtm = Autodesk.AutoCAD.Runtime;
#elif NC

using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
//using Teigha.Runtime;
using App = HostMgd.ApplicationServices;
using Rtm = Teigha.Runtime;
#endif





[assembly: Rtm.ExtensionApplication(typeof(dRz.SpecSPDS.Cad.EntryPoint))]

namespace dRz.SpecSPDS.Cad
{
    partial class EntryPoint : Rtm.IExtensionApplication
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [Rtm.CommandMethod("инит")]
        [Description("проверка работы лога")]
        public void Initialize()
        {

#if DEBUG
            //debug internal nlog
            InternalLoggerDiagnostic.InternalLoggerInit();
#endif

            //setup environment
            ApplicationHost.Start();

            log.Info("nanoCAD 23.1 перед LogBootstrap");
             NLog.Config.LoggingConfiguration? config = LogManager.Configuration;
            LogBootstrap.Init();
            config = LogManager.Configuration;
            log.Info("nanoCAD 23.1 после LogBootstrap");

            //NlogTest.TestLog();

            Trace.WriteLine("nanoCAD 23.1 загружен Ok");

            log.Info("nlogInit");

            Loader.HelloSpec();
            //throw new NotImplementedException();
        }

        public void Terminate()
        {
            log.Info("Shutdown");
            //loger stop
            LogManager.Shutdown();

        }
    }

    class Loader
    {
        internal static void HelloSpec()
        {
            Document doc = App.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }

            Editor ed = doc.Editor;

            ed.WriteMessage($"Hello Spec SPDS for nanoCAD 23-26");
        }
    }

   

}