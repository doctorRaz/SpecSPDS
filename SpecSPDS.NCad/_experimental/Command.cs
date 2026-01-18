using dRz.nCad.Loader;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using System.ComponentModel;
using App = HostMgd.ApplicationServices;
using Rtm = Teigha.Runtime;



//[assembly: Rtm.CommandClass(typeof(drz.PlotSPDSn.Starter.NET.Command.Command))]

namespace drz.nCad.Loader.Experimental
{
    partial class Command //:Rtm.IExtensionApplication
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
                       

            EntryPoint entryPoint = new EntryPoint();

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
