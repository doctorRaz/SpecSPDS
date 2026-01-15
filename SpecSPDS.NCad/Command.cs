using App = HostMgd.ApplicationServices;
using Ed = HostMgd.EditorInput;
using Rtm = Teigha.Runtime;

using System.Reflection;

using Db = Teigha.DatabaseServices;
using System.ComponentModel;
using HostMgd.EditorInput;
//using System.Reflection.Metadata;
using HostMgd.ApplicationServices;
//using drz.PlotSPDSn.Starter.NET.EntryPoint;

[assembly: Rtm.CommandClass(typeof(drz.PlotSPDSn.Starter.NET.Command.Command))]

namespace drz.PlotSPDSn.Starter.NET.Command
{
    partial class Command:Rtm.IExtensionApplication
    {
        [Rtm.CommandMethod("ll", Rtm.CommandFlags.UsePickSet)]
        [Description("тест загрузки")]
        public static void TestLoad()
        {
            App.Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }

            Editor ed = doc.Editor;

            ed.WriteMessage($"\nHello word");
                       

            EntryPoint.EntryPoint entryPoint = new EntryPoint.EntryPoint();

            entryPoint.Initialize();

        }

        public void Initialize()
        {
            App.Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }

            Editor ed = doc.Editor;

            ed.WriteMessage($"\nStarter");
            //throw new NotImplementedException();
        }

        public void Terminate()
        {
            //throw new NotImplementedException();
        }
    }
}
