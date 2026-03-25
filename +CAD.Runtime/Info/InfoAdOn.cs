using System;
using System.IO;
using System.Reflection;

namespace dRz.CAD.Runtime.Info
{
    /// <summary>
    /// Runtime environment for loader assembly.
    /// Immutable static context.
    /// </summary>
    public class InfoAdOn
    {
        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $@"{ProductName} v{AssemblyVersion}; Module: {ProductTitle}; {InformationalVersion}";
        }

        public string LongString()
        {
            return $@"{ProductName} v{AssemblyVersion}; Title: {ProductTitle}; {InformationalVersion}; File: {AssemblyPath}";
        }

        static InfoAdOn()
        {
            Assembly assembly = typeof(InfoAdOn).Assembly;

            AssemblyPath = assembly.Location;
            AssemblyDirectory = Path.GetDirectoryName(AssemblyPath)!;
            FileName = Path.GetFileNameWithoutExtension(AssemblyPath);

            AssemblyVersion = assembly.GetName().Version!;

            InformationalVersion =
                        assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                        ?? "Unknow";
            ProductName =
                assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product
                ?? ExtractProductPrefix(FileName);

            ProductTitle =
                assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title
                ?? FileName;

            AppDataProductPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ProductName);

            AppDataProductLogPath = Path.Combine(AppDataProductPath, "Logs");

            NLogConfigPath = Path.Combine(AssemblyDirectory, _nLogConfigFileName);
        }

        // -------------------- Public API --------------------


        /// <summary>
        /// Gets the assembly path.
        /// </summary>
        /// <value>
        /// The assembly path.
        /// </value>
        public static string AssemblyPath { get; }

        /// <summary>
        /// Gets the assembly directory.
        /// </summary>
        /// <value>
        /// The assembly directory.
        /// </value>
        public static string AssemblyDirectory { get; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public static string FileName { get; }

        /// <summary>
        /// Gets the name of the product.
        /// </summary>
        /// <value>
        /// The name of the product.
        /// </value>
        public static string ProductName { get; }

        /// <summary>
        /// Gets the informational version attribute.
        /// </summary>
        /// <value>
        /// The informational version attribute.
        /// </value>
        public static string InformationalVersion { get; }

        /// <summary>
        /// Gets the product title.
        /// </summary>
        /// <value>
        /// The product title.
        /// </value>
        public static string ProductTitle { get; }

        /// <summary>
        /// Gets the application data product path.
        /// </summary>
        /// <value>
        /// The application data product path.
        /// </value>
        public static string AppDataProductPath { get; }

        /// <summary>
        /// Gets the application data product log path.
        /// </summary>
        /// <value>
        /// The application data product log path.
        /// </value>
        public static string AppDataProductLogPath { get; }

        /// <summary>
        /// Gets the n log configuration path.
        /// </summary>
        /// <value>
        /// The n log configuration path.
        /// </value>
        public static string NLogConfigPath { get; }

        public static Version AssemblyVersion { get; }

        // -------------------- Helpers --------------------

        /// <summary>
        /// Первая часть имени сборки до точки
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string ExtractProductPrefix(string fileName)
        {
            int dotIndex = fileName.IndexOf('.');
            return dotIndex > 0
                ? fileName.Substring(0, dotIndex)
                : fileName;
        }

        private const string _nLogConfigFileName = "drzNLog.dll.nlog";
    }
}