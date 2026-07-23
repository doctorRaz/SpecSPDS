using System;
using System.IO;

namespace drz.Updater.Services
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
                throw new FileNotFoundException("Файл не найден", sourceFile);

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
                return backup;

            // file(1).bak ... file(10).bak
            for (int i = 1; ; i++)
            {
                backup = Path.Combine(
                    directory,
                    $"{name}({i}).bak");

                if (!File.Exists(backup))
                    return backup;
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
                return false;

            foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                string relative = file.RelativeTo(sourceDirectory);

                string target = Path.Combine(targetDirectory, relative);

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
    }

    public static class PathEx
    {
        /// <summary>Relatives to.</summary>
        /// <param name="path">The path.</param>
        /// <param name="basePath">The base path.</param>
        /// <returns></returns>
        public static string RelativeTo(this string path, string basePath)
        {
            return GetRelativePath(basePath, path);
        }

        /// <summary>Gets the relative path.</summary>
        /// <param name="basePath">The base path.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static string GetRelativePath(string basePath, string path)
        {
            var baseUri = new Uri(
                AppendDirectorySeparator(basePath));

            var pathUri = new Uri(path);

            return Uri.UnescapeDataString(
                baseUri.MakeRelativeUri(pathUri).ToString()
            ).Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>Appends the directory separator.</summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static string AppendDirectorySeparator(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                return path + Path.DirectorySeparatorChar;

            return path;
        }
    }
}