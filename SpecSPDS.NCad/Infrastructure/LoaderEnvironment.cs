using System;
using System.IO;
using System.Reflection;

namespace dRz.Loader.Cad.Infrastructure
{
    public class LoaderEnvironment
    {
        //************ зависит от порядка свойств!!!! *************

        // Сборка — дешево
        private static readonly Assembly _assembly =
            typeof(LoaderEnvironment).Assembly;

        // Полный путь к DLL — дешево
        private static readonly string _assemblyPath =
            _assembly.Location;

        // Папка сборки
        private static readonly string _assemblyDirectory =
            Path.GetDirectoryName(_assemblyPath)!;

        // Имя файла без расширения (Specspds.ncad)
        private static readonly string _fileName =
            Path.GetFileNameWithoutExtension(_assemblyPath);

        // ProductName = часть ДО первой точки (Specspds)
        public static readonly string ProductName =
            ExtractProductPrefix(_fileName);

        // %AppData%\Product
        public static readonly string AppDataProductPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ProductName);

        // %AppData%\Product
        public static readonly string AppDataProductLogPath = Path.Combine(AppDataProductPath, "Logs");

        public static readonly string NLogConfigPath = Path.Combine(_assemblyDirectory, _nLogConfigFileName);

        //----------------------------

        //имя лог файла
        //private const string _nLogConfigFileName = "NLog.dll.nlog";
        private const string _nLogConfigFileName = "NLog.dll.test.nlog";

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
