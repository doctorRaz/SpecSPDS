
using Application = HostMgd.ApplicationServices.Application;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teigha.Runtime;
using Teigha.DatabaseServices;

//https://adn-cis.org/forum/index.php?topic=10308.msg47463#msg47463
namespace LineWeightChange
{
    public class Commands
    {
        [CommandMethod("ASYNCMETHOD")]
        public async void AsyncMethod()
        {
            string result = Browser();
            if (result != "")
            {
                bool taskResult = await Task.Run(() => AsyncWorkWithDirectories(result));
                if (taskResult)
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCOMPLETE\n");
            }
        }
 
        [CommandMethod("PARALLELMETHOD")]
        public void ParallelMethod()
        {
            string result = Browser();
            if (result != "")
            {
                Parallel.Invoke(() => ParallelWorkWithDirectories(result));
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCOMPLETE\n");
            }
        }
 
        [CommandMethod("STANDARDMETHOD")]
        public void StandardMethod()
        {
            string result = Browser();
            if (result != "")
            {
                StandardWorkWithDirectories(result);
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nCOMPLETE\n");
            }
        }
 
        private void WorkWithDrawing(string path)
        {
            using (Database database = new Database(false, true))
            {
                database.ReadDwgFile(path, FileOpenMode.OpenForReadAndWriteNoShare, true, null);
                using (Transaction tr = database.TransactionManager.StartTransaction())
                {
                    LayerTable layerTable = tr.GetObject(database.LayerTableId, OpenMode.ForWrite) as LayerTable;
                    if (layerTable.Has("SolidThick"))
                    {
                        LayerTableRecord layer = tr.GetObject(layerTable["SolidThick"], OpenMode.ForWrite) as LayerTableRecord;
                        layer.LineWeight = LineWeight.LineWeight050;
                    }
                    if (layerTable.Has("SolidThin"))
                    {
                        LayerTableRecord layer = tr.GetObject(layerTable["SolidThin"], OpenMode.ForWrite) as LayerTableRecord;
                        layer.LineWeight = LineWeight.ByLineWeightDefault;
                    }
                    tr.Commit();
                }
                SecurityParameters security = Application.DocumentManager.MdiActiveDocument.Database.SecurityParameters;
                database.SaveAs(path, true, DwgVersion.AC1024, security);
            }
        }
 
        private async Task<bool> AsyncWorkWithDirectories(string path)
        {
            bool result = true;
            DirectoryInfo currentDirectory = new DirectoryInfo(path);
            DirectoryInfo[] directories = currentDirectory.GetDirectories();
            foreach (DirectoryInfo directory in directories)
            {
                FileInfo[] files = directory.GetFiles();
                foreach (FileInfo file in files)
                    if (file.Extension == ".dwg")
                        await Task.Run(() => WorkWithDrawing(file.FullName));
                result = await Task.Run(() => AsyncWorkWithDirectories(directory.FullName));
            }
            return result;
        }
 
        private void ParallelWorkWithDirectories(string path)
        {
            DirectoryInfo currentDirectory = new DirectoryInfo(path);
            DirectoryInfo[] directories = currentDirectory.GetDirectories();
            foreach (DirectoryInfo directory in directories)
            {
                FileInfo[] files = directory.GetFiles();
                foreach (FileInfo file in files)
                    if (file.Extension == ".dwg")
                        Parallel.Invoke(() => WorkWithDrawing(file.FullName));
                Parallel.Invoke(() => ParallelWorkWithDirectories(directory.FullName));
            }
        }
 
        private void StandardWorkWithDirectories(string path)
        {
            DirectoryInfo currentDirectory = new DirectoryInfo(path);
            DirectoryInfo[] directories = currentDirectory.GetDirectories();
            foreach (DirectoryInfo directory in directories)
            {
                FileInfo[] files = directory.GetFiles();
                foreach (FileInfo file in files)
                    if (file.Extension == ".dwg")
                        WorkWithDrawing(file.FullName);
                StandardWorkWithDirectories(directory.FullName);
            }
        }
 
        private string Browser()
        {
            FolderBrowserDialog browser = new FolderBrowserDialog
            {
                Description = "Some description",
                ShowNewFolderButton = false
            };
            browser.ShowDialog();
            return browser.SelectedPath;
        }
    }
}