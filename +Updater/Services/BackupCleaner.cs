using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace drz.Updater.Services
{
    /// <summary>
    /// Выполняет очистку каталога от резервных файлов и удаляет пустые папки.
    /// </summary>
    /// <remarks>
    /// Удаляются:
    /// <list type="bullet">
    /// <item><description>файлы с расширением <c>.bak</c>;</description></item>
    /// <item><description>копии файлов вида <c>Имя(1).dll</c>, <c>Имя(2).json</c> и т.п.</description></item>
    /// </list>
    /// После удаления файлов рекурсивно удаляются пустые каталоги.
    /// </remarks>
    public class BackupCleaner

    {
        #region Private Fields

        /// <summary>
        /// Регулярное выражение для определения файлов-копий,
        /// созданных с добавлением числового суффикса в круглых скобках,
        /// например: <c>Plugin(1).dll</c>.
        /// </summary>
        private static readonly Regex _backupCopyRegex =
                                new(@"\(\d+\)\.[^.]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Удаляет резервные файлы и их копии из указанного каталога и всех вложенных каталогов.
        /// </summary>
        /// <param name="directoryPath">
        /// Путь к каталогу, в котором необходимо выполнить очистку.
        /// </param>
        /// <remarks>
        /// Удаляются:
        /// <list type="bullet">
        /// <item><description>файлы с расширением <c>.bak</c>;</description></item>
        /// <item><description>файлы вида <c>Имя(число).Расширение</c>.</description></item>
        /// </list>
        /// После удаления файлов автоматически удаляются все пустые каталоги.
        /// </remarks>
        public static void DeleteBackupFiles(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return;

            try
            {
                // EnumerateFiles лениво проходит файлы рекурсивно
                foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories))
                {
                    string fileName = Path.GetFileName(filePath);

                    string extension = Path.GetExtension(filePath);

                    if (extension.Equals(".bak", StringComparison.OrdinalIgnoreCase) || _backupCopyRegex.IsMatch(fileName))
                    {
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch (Exception ex)
                        {
                            //todo : добавить логирование ошибок удаления файлов
                            //Logger.Warn(ex);
                        }
                    }
                }
            }
            catch { }

            //удалить пустые каталоги
            DeleteEmptyDirectories(directoryPath);
        }

        /// <summary>
        /// Рекурсивно удаляет все пустые каталоги, начиная с указанного.
        /// </summary>
        /// <param name="directoryPath">
        /// Путь к каталогу, в котором необходимо выполнить очистку.
        /// </param>
        /// <remarks>
        /// Сначала обрабатываются вложенные каталоги, затем при отсутствии файлов
        /// и подкаталогов удаляется текущий каталог.
        /// </remarks>
        public static void DeleteEmptyDirectories(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return;

            foreach (var directory in Directory.EnumerateDirectories(directoryPath))
            {
                DeleteEmptyDirectories(directory);
                try
                {
                    if (!Directory.EnumerateFileSystemEntries(directory).Any())
                        Directory.Delete(directory);
                }
                catch (Exception ex)
                {
                    //todo : добавить логирование ошибок удаления каталогов
                    //Logger.Warn(ex);
                }
            }
        }

        #endregion Public Methods
    }
}