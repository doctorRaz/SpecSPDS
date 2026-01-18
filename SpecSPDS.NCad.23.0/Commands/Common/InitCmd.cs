using dRz.SpecSPDS.Cad.Commands.Test;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using NLog;
using NLog.Common;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Teigha.Runtime;
using App = HostMgd.ApplicationServices;

namespace dRz.SpecSPDS.Cad.Commands
{

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

    partial class InitCmd : IExtensionApplication
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        [CommandMethod("инит")]
        [Description("проверка работы лога")]
        public void Initialize()
        {

            InternalLogger.LogLevel = LogLevel.Trace;

            InternalLogger.LogWriter = new DebugTextWriter();


            LogManager.ThrowExceptions = true;
            LogManager.ThrowConfigExceptions = true;

              InternalLogger.Trace("Cad.INIT.EntryPoint.Initialize()");

            LogBootstrap.Init();

            //LogManager.Setup().LoadConfigurationFromFile(@"d:\@Developers\Programmers\!NET\!SpecSPDS\bin.NET\Debug\NLog.config");

            InternalLogger.Trace("23.1 инит");

            log.Info("nanoCAD 23.1 загружен");

            NlogTest.TestLog();

            Trace.WriteLine("nanoCAD 23.1 загружен");

            Loader.HelloSpec();
            //throw new NotImplementedException();
        }

        public void Terminate()
        {

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