using System;
using System.Collections.Generic;
using System.IO;

namespace dRz.Updater.Services
{
    /// <summary>
    /// Installer
    /// </summary>
    public static class Installer
    {
        /// <summary>Moves the with backup.</summary>
        /// <param name="sourceFile">The source file.</param>
        /// <param name="targetDirectory">The target directory.</param>
        /// <exception cref="System.IO.FileNotFoundException">Файл не найден</exception>
        public static void MoveWithBackup(string sourceFile, string targetDirectory)
        {
            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException("Файл не найден", sourceFile);
            }

            Directory.CreateDirectory(targetDirectory);

            string targetFile = Path.Combine(
                targetDirectory,
                Path.GetFileName(sourceFile));

            if (File.Exists(targetFile))//todo переименование вынести в метод
            {
                string backupFile = GetBackupName(targetFile);

                File.Move(targetFile, backupFile);//rename
            }

            File.Move(sourceFile, targetFile);//move
        }

        /// <summary>Gets the name of the backup.</summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        private static string GetBackupName(string file)
        {
            string directory = Path.GetDirectoryName(file)!;

            string name = Path.GetFileName(file);

            // file.bak
            string backup = Path.Combine(directory, name + ".bak");

            if (!File.Exists(backup))
            {
                return backup;
            }

            // file(1).bak ... file(10).bak
            for (int i = 1; ; i++)
            {
                backup = Path.Combine(
                    directory,
                    $"{name}({i}).bak");

                if (!File.Exists(backup))
                {
                    return backup;
                }
            }
        }

        /// <summary>
        /// Переименовывает все файлы каталога в резервные копии, сохраняя структуру каталогов.
        /// При ошибке пытается вернуть уже переименованные файлы исходным именам.
        /// </summary>
        /// <param name="directory">Каталог установленного пакета или модуля.</param>
        /// <returns><see langword="true"/>, если все файлы переименованы; иначе <see langword="false"/>.</returns>
        public static bool RenameDirectoryFilesWithBackup(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return false;
            }

            // Материализуем список до изменения имён файлов.
            string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            List<(string Original, string Backup)> renamedFiles = new();

            try
            {
                foreach (string file in files)
                {
                    string backup = GetBackupName(file);
                    File.Move(file, backup);
                    renamedFiles.Add((file, backup));
                }

                return true;
            }
            catch
            {
                // Откатываем переименования в обратном порядке.
                for (int i = renamedFiles.Count - 1; i >= 0; i--)
                {
                    (string original, string backup) = renamedFiles[i];

                    try
                    {
                        if (File.Exists(backup) && !File.Exists(original))
                        {
                            File.Move(backup, original);
                        }
                    }
                    catch
                    {
                        // Сохраняем исходную ошибку; неудачный откат требует диагностики.
                    }
                }

                return false;
            }
        }

        /// <summary>Moves the directory files with backup.</summary>
        /// <param name="sourceDirectory">The source directory.</param>
        /// <param name="targetDirectory">The target directory.</param>
        public static bool MoveDirectoryFilesWithBackup(
        string sourceDirectory,
        string targetDirectory)
        {
            if (!Directory.Exists(sourceDirectory))
            {
                return false;
            }

            foreach (string file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(sourceDirectory, file);

                string target = GetSafeTargetPath(targetDirectory, relative);

                Directory.CreateDirectory(Path.GetDirectoryName(target)!);

                MoveWithBackup(file, Path.GetDirectoryName(target)!);
            }

            try
            {
                //delete  source Directory recursively
                Directory.Delete(sourceDirectory, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверяет, что относительный путь не выходит за пределы каталога установки.
        /// </summary>
        private static string GetSafeTargetPath(string targetDirectory, string relativePath)
        {
            string root = Path.GetFullPath(targetDirectory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            string target = Path.GetFullPath(Path.Combine(root, relativePath));

            if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Архив содержит путь за пределами каталога установки.");

            return target;
        }
    }
}