using System;
using System.IO;
using System.Reflection;

namespace dRz.SpecSpds.Test._draft
{
    public class _LoaderEnvironment
    {
        // Сборка — дешево
        private static readonly Assembly _assembly =
            typeof(_LoaderEnvironment).Assembly;

        // Полный путь к DLL — дешево
        public static readonly string AssemblyPath =
            _assembly.Location;

        // Папка сборки
        public static readonly string AssemblyDirectory =
            Path.GetDirectoryName(AssemblyPath)!;

        // Имя файла без расширения (Specspds.ncad)
        private static readonly string _fileName =
            Path.GetFileNameWithoutExtension(AssemblyPath);

        // ProductName = часть ДО первой точки (Specspds)
        public static readonly string ProductName =
            ExtractProductPrefix(_fileName);

        // %AppData%\Product
        public static readonly string AppDataProductPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ProductName);

        // -------------------------

        private static string ExtractProductPrefix(string fileName)
        {
            int dotIndex = fileName.IndexOf('.');
            return dotIndex > 0
                ? fileName.Substring(0, dotIndex)
                : fileName;
        }
    }
}
