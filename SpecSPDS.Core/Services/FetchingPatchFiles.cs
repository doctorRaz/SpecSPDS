using dRz.SpecSPDS.Core.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace dRz.SpecSPDS.Core.Services
{
    public class FetchingPatchFiles
    {
        /// <summary>
        /// Определяет способ получения файлов, выбор каталога или фалов
        /// </summary>
        /// <param name="space">The space.</param>
        /// <returns>список путей к файлам</returns>
        public static List<string> GetFiles(Space space)
        {

            if (space == Space.Folder || space == Space.SubFolder)
            {
                string description = "Выберите папку";

                if (space == Space.SubFolder) description += " (обработаются файлы в подпапках)";

                //собрать файлы из каталога
                return GetFilesOfDir(Browser(description), space == Space.SubFolder);
            }
            else
            {
                return Files();
            }


        }

        /// <summary>
        /// путь к папке
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns>путь к папке</returns>
        static string Browser(string description)
        {
            //https://autolisp.ru/2024/05/23/nanocad-net-select-folder/
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = description;
                dlg.UseDescriptionForTitle = true;
                //dlg.Multiselect  = true;
                // Остальные настройки

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (!string.IsNullOrWhiteSpace(dlg.SelectedPath))
                    {
                        return dlg.SelectedPath;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Не выбран каталог!",
                               "SpecSPDS",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
                }

                return string.Empty;
            }

        }

        /// <summary>
        /// Выбор файлов чертежей.
        /// </summary>
        /// <returns>Список путей к файлам</returns>
        static List<string> Files()
        {
            //todo обернуть try canch???

            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Multiselect = true;
                dlg.Title = "Выберите файлы чертежей";
                dlg.Filter = "Files dwg|*.dwg";
                dlg.RestoreDirectory = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // Проверка расширений файлов https://chat.deepseek.com
                    List<string> validFiles = dlg.FileNames
                        .Where(file => Path.GetExtension(file).Equals(".dwg", StringComparison.InvariantCultureIgnoreCase))
                        .ToList();

                    if (validFiles.Count > 0) return validFiles;

                }

                System.Windows.MessageBox.Show("Ничего не выбрано!",
                       "SpecSPDS",
                       MessageBoxButton.OK,
                       MessageBoxImage.Information);

                return new List<string>();

            }

        }

        /// <summary>
        /// Получить пути файлов из каталога
        /// </summary>
        /// <param name="sPath">The s path.</param>
        /// <param name="WithSubfolders">if set to <c>true</c> [with subfolders].</param>
        /// <param name="sSerchPatern">The s serch patern.</param>
        /// <returns>Список файлов</returns>
      public  static List<string> GetFilesOfDir(string sPath, bool WithSubfolders = false, string sSerchPatern = "*.dwg")
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
                //todo ВСЕ диалоги переделать на интерфейс сообщений
#if NC || AC
                Cad.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n" + ex.Message);
#else
                System.Windows.MessageBox.Show(ex.Message);
#endif
                return new List<string>();
            }

        }





    }
}
