using System;
using System.IO;
using System.Reflection;

namespace dRz.Loader.Cad.Infrastructure
{
    public class LoaderEnvironment
    {
        //************ зависит от порядка свойств!!!! *************

        /// <summary>
        /// Сборка — дешево
        /// </summary>
        private static readonly Assembly _assembly =
            typeof(LoaderEnvironment).Assembly;



        /// <summary>
        /// Полный путь к DLL — дешево
        /// </summary>
        private static readonly string _assemblyPath =
            _assembly.Location;

        /// <summary>
        /// Папка сборки
        /// </summary>
        private static readonly string _assemblyDirectory =
            Path.GetDirectoryName(_assemblyPath)!;

        /// <summary>
        /// Имя файла без расширения (Specspds.ncad)
        /// </summary>
        private static readonly string _fileName =
            Path.GetFileNameWithoutExtension(_assemblyPath);


        /// <summary>
        /// The s product GetCustomAttribute
        /// </summary>
        public static readonly string sProduct = _assembly
                                                 .GetCustomAttribute<AssemblyProductAttribute>()?
                                                 .Product
                                                 ?? _assembly.GetName().Name!;

        /// <summary>
        /// часть ДО первой точки (Specspds) 
        /// </summary>
        public static readonly string ProductName = ExtractProductPrefix(_fileName);

        /// <summary>
        /// %AppData%\Product
        /// </summary>
        public static readonly string AppDataProductPath =
                                        Path.Combine(
                                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                            ProductName);

        /// <summary>
        /// %AppData%\Product\Logs
        /// </summary>
        public static readonly string AppDataProductLogPath = Path.Combine(AppDataProductPath, "Logs");

        public static readonly string NLogConfigPath = Path.Combine(_assemblyDirectory, _nLogConfigFileName);

        //----------------------------

        /// <summary>
        /// имя лог файла
        /// </summary>
        //private const string _nLogConfigFileName = "NLog.dll.nlog";
        private const string _nLogConfigFileName = "NLog.dll.test.nlog";

        // -------------------------

        /// <summary>
        /// Extracts the product prefix.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private static string ExtractProductPrefix(string fileName)
        {
            int dotIndex = fileName.IndexOf('.');
            return dotIndex > 0
                ? fileName.Substring(0, dotIndex)
                : fileName;
        }
    }
}
