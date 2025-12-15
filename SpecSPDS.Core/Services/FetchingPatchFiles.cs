using dRz.SpecSPDS.Core.Enums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dRz.SpecSPDS.NCad.Services
{
    public class FetchingPatchFiles
    {
        public static List<string> GetFiles(Space space)
        {
            //string folderPatch = string.Empty;
            //string[] filePatchs = new string[] { };

            if (space == Space.Folder || space == Space.SubFolder)
            {
                //собрать файлы из каталога
                return GetFilesOfDir(Browser(), space == Space.SubFolder);
            }
            else
            {           
                return new List<string>();
            }


        }

        static string Browser()
        {
            //https://autolisp.ru/2024/05/23/nanocad-net-select-folder/
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Выберите каталог";
                dlg.UseDescriptionForTitle = true;
                // Остальные настройки

                dlg.ShowDialog();

                if (!string.IsNullOrWhiteSpace(dlg.SelectedPath))
                {
                    return dlg.SelectedPath;
                }
                return string.Empty;
            }

        }


        static string[] Files()
        {

            return new string[] { };
        }

        static List<string> GetFilesOfDir(string sPath, bool WithSubfolders = false, string sSerchPatern = "*.dwg")
        {
            try
            {
                if (Directory.Exists(sPath))
                {
                    return Directory.GetFiles(sPath,
                                                sSerchPatern,
                                                (WithSubfolders
                                                ? SearchOption.AllDirectories
                                                : SearchOption.TopDirectoryOnly)).ToList<string>();
                }
                else
                {

                    return new List<string>();
                }
            }
            catch (System.Exception ex)
            {
                //todo переделать на интерфейс сообщений
#if NC || AC
                Cad.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n" + ex.Message);
#else
                MessageBox.Show(ex.Message);
#endif
                return new List<string>();
            }

        }




        //Space _space;
    }
}
