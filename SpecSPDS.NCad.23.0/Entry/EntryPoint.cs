using NLog.Common;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using dRz.SpecSPDS.Cad.Commands.Test;
using NLog;
using dRz.SpecSPDS.Cad.Application;


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
            InternalLogger.LogWriter = new DebugTextWriter();

            InternalLogger.Info("EntryPoint.Initialize() start");

            ApplicationHost.Start();

            InternalDiagnostic.InitInternalLogger();

            log.Info("nanoCAD 23.1 перед LogBootstrap");

            LogBootstrap.Init();

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

    /// <summary>
    /// отладочная информация из nLog в output VS
    /// </summary>
    /// <seealso cref="System.IO.TextWriter" />
    sealed class DebugTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string? value)
        {
            Debug.WriteLine(value);
        }

        public override void Write(string? value)
        {
            Debug.Write(value);
        }
    }

    internal class InternalDiagnostic
    {
        internal static void InitInternalLogger()
        {
            #region InternalLogger configure

            InternalLogger.LogLevel = LogLevel.Info;

            InternalLogger.LogWriter = new DebugTextWriter();

            LogManager.ThrowExceptions = true;

            LogManager.ThrowConfigExceptions = true;

            InternalLogger.Info("InternalLogger.Initialize()");

            #endregion
        }
    }

}