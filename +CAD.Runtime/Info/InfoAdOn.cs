using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace dRz.CAD.Runtime.Info
{
    /// <summary>
    /// Runtime environment for specific assembly (addon/module).
    /// Immutable per-assembly context.
    /// </summary>
    public sealed class InfoAdOn
    {
        private const string _nLogConfigFileName = "drzNLog.dll.nlog";

        private static readonly ConcurrentDictionary<Assembly, InfoAdOn> _cache = new();

        /// <summary>
        /// Gets the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static InfoAdOn Get(Assembly assembly)
        {
            return _cache.GetOrAdd(assembly, asm => new InfoAdOn(asm));
        }

        public static InfoAdOn Get(Type type)
        {
            return Get(type.Assembly);
        }

        private InfoAdOn(Assembly assembly)
        {


            Assembly = assembly;
            try
            {
                AssemblyPath = assembly.Location;
            }
            catch
            {
                AssemblyPath = string.Empty;
            }

            AssemblyDirectory = !string.IsNullOrEmpty(AssemblyPath)
                ? Path.GetDirectoryName(AssemblyPath) ?? string.Empty
                : string.Empty;

            FileName = !string.IsNullOrEmpty(AssemblyPath)
                ? Path.GetFileNameWithoutExtension(AssemblyPath)
                : assembly.GetName().Name ?? "Unknown";

            FilePrefix = ExtractProductPrefix(FileName);

            AssemblyVersion = assembly.GetName().Version ?? new Version(0, 0, 0, 0);

            InformationalVersion =
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? "Unknown";

            ProductName =
                assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product
                ?? FilePrefix;

            ProductTitle =
                assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title
                ?? FileName;

            FileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version
                ?? "Unknown";

            Copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright
                ?? "Unknown";

            Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description
                ?? "Unknown";

            // ---AppData ---
            string appData =  Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (string.IsNullOrEmpty(appData))
                appData = Path.GetTempPath(); // fallback

            AppDataProductPath = Path.Combine(appData, ProductName);

            AppDataProductLogPath = Path.Combine(AppDataProductPath, "Logs");

            // --- Nlog ---
            NLogConfigPath = !string.IsNullOrEmpty(AssemblyDirectory)
                ? Path.Combine(AssemblyDirectory, _nLogConfigFileName)
                : _nLogConfigFileName;
        }

        // -------------------- Factory --------------------

        /// <summary>
        /// Froms the assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">assembly</exception>
        public static InfoAdOn FromAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            return new InfoAdOn(assembly);
        }

        /// <summary>
        /// Froms the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static InfoAdOn FromType(Type type)
            => new InfoAdOn(type.Assembly);

        /// <summary>
        /// НЕ рекомендуется для production (может вернуть не ту сборку)
        /// </summary>
        public static InfoAdOn FromCallingAssembly()
            => new InfoAdOn(Assembly.GetCallingAssembly());

        // -------------------- Public API --------------------

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        /// <value>
        /// The assembly.
        /// </value>
        public Assembly Assembly { get; }

        /// <summary>
        /// Gets the assembly path.
        /// </summary>
        /// <value>
        /// The assembly path.
        /// </value>
        public string AssemblyPath { get; }

        /// <summary>
        /// Gets the assembly directory.
        /// </summary>
        /// <value>
        /// The assembly directory.
        /// </value>
        public string AssemblyDirectory { get; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; }

        /// <summary>
        /// Gets the file prefix.
        /// </summary>
        /// <value>
        /// The file prefix.
        /// </value>
        public string FilePrefix { get; }

        /// <summary>
        /// Gets the name of the product.
        /// </summary>
        /// <value>
        /// The name of the product.
        /// </value>
        public string ProductName { get; }

        /// <summary>
        /// Gets the product title.
        /// </summary>
        /// <value>
        /// The product title.
        /// </value>
        public string ProductTitle { get; }

        /// <summary>
        /// Gets the informational version.
        /// </summary>
        /// <value>
        /// The informational version.
        /// </value>
        public string InformationalVersion { get; }

        /// <summary>
        /// Gets the assembly version.
        /// </summary>
        /// <value>
        /// The assembly version.
        /// </value>
        public Version AssemblyVersion { get; }

        /// <summary>
        /// Gets the application data product path.
        /// </summary>
        /// <value>
        /// The application data product path.
        /// </value>
        public string AppDataProductPath { get; }

        /// <summary>
        /// Gets the application data product log path.
        /// </summary>
        /// <value>
        /// The application data product log path.
        /// </value>
        public string AppDataProductLogPath { get; }

        /// <summary>
        /// Gets the n log configuration path.
        /// </summary>
        /// <value>
        /// The n log configuration path.
        /// </value>
        public string NLogConfigPath { get; }

        /// <summary>
        /// Gets the assembly file version.
        /// </summary>
        public string FileVersion { get; }

        /// <summary>
        /// Gets the assembly copyright information.
        /// </summary>
        public string Copyright { get; }

        /// <summary>
        /// Gets the assembly description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{ProductName} v{AssemblyVersion}; Module: {ProductTitle}; {InformationalVersion}";
        }

        /// <summary>
        /// Converts Longs the string.
        /// </summary>
        /// <returns></returns>
        public string LongString()
        {
            return @$"LongString
    {ProductName} v{AssemblyVersion}
    Title: {ProductTitle}
    InformationalVersion: {InformationalVersion}
    FileName: {FileName}
    FilePrefix: {FilePrefix}
    File: {AssemblyPath}
    Copyright:{Copyright}
    Description: {Description}
    FileVersion: {FileVersion}
";
        }

        // -------------------- Helpers --------------------

        private static string ExtractProductPrefix(string fileName)
        {
            int dotIndex = fileName.IndexOf('.');
            return dotIndex > 0
                ? fileName.Substring(0, dotIndex)
                : fileName;
        }
    }
}