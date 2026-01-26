using System;
using System.IO;
using System.Reflection;

namespace dRz.Loader.Cad.Infrastructure
{
    public  class LoaderEnvironment
    {
        //todo зависит от порядка свойств!!!!

        // ProductName = часть ДО первой точки (Specspds)
        public static readonly string ProductName =
            ExtractProductPrefix(_fileName);

        // %AppData%\Product
        public static readonly string AppDataProductPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ProductName);

        // Папка сборки
        public static readonly string AssemblyDirectory =
            Path.GetDirectoryName(AssemblyPath)!;

        // Полный путь к DLL — дешево
        public static readonly string AssemblyPath =
            _assembly.Location;


        //----------------------------
        // Сборка — дешево
        private static readonly Assembly _assembly =
            typeof(LoaderEnvironment).Assembly;

        // Имя файла без расширения (Specspds.ncad)
        private static readonly string _fileName =
            Path.GetFileNameWithoutExtension(AssemblyPath);

        //имя лог файла
        //private const string nLogConfigFileName = "NLog.dll.nlog";

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
