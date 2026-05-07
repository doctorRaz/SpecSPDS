using drz.Abstractions.Infrastructure;
using System;
using System.Reflection;
using System.IO;

namespace drz.Infrastructure.Infrastructure
{
    public class ApplicationInfo_NEW : IApplicationInfo_NEW
    {
        //private readonly Assembly _assembly;

        //private IntPtr _handle;

        #region Public Constructors

        public ApplicationInfo_NEW(Assembly assembly)
        {
            AssemblyName assemblyName = assembly.GetName();

            AssemblyPath = assembly.Location ?? string.Empty;

            if (!string.IsNullOrEmpty(AssemblyPath))
            {
                AssemblyDirectory = Path.GetDirectoryName(AssemblyPath) ?? string.Empty;

                FileName = Path.GetFileNameWithoutExtension(AssemblyPath);
            }
            else
            {
                AssemblyDirectory = string.Empty;

                FileName = assemblyName.Name ?? "Unknown";
            }

            AssembleFullName = assembly.FullName;

            AssemblyVersion = assemblyName.Version ?? new Version(0, 0, 0, 0);

            FilePrefix = ExtractProductPrefix(FileName);

            BuildDate = ComputeBuildDate(assembly, out bool isAuto);

            IsAutoVersion = isAuto;

            InformationalVersion =
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? "Unknown";

            ProductName =
                assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product
                ?? FilePrefix;

            ProductTitle =
                assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title
                ?? FileName;

            FileVersion =
                assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version
                ?? "Unknown";

            Copyright =
              assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright
                ?? "Unknown";

            Description =
               assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description
                ?? "Unknown";

            // ---AppData ---
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (string.IsNullOrEmpty(appData))
            {
                appData = Path.GetTempPath(); // fallback
            }

            AppDataProductPath = Path.Combine(appData, ProductName);

            AppDataProductLogPath = Path.Combine(AppDataProductPath, "Logs");

            TitlePrefix = $"{ProductName} v.{AssemblyVersion} : ";
        }

        #endregion Public Constructors

        #region Public Properties

        public string AppDataProductLogPath { get; }
        public string AppDataProductPath { get; }
        public string AssembleFullName { get; }
        public string AssemblyDirectory { get; }
        public string AssemblyPath { get; }
        public Version AssemblyVersion { get; }
        public DateTime BuildDate { get; }
        public string Copyright { get; }
        public string Description { get; }
        public string FileName { get; }
        public string FilePrefix { get; }
        public string FileVersion { get; }
        public string InformationalVersion { get; }
        public bool IsAutoVersion { get; }
        public string ProductName { get; }
        public string ProductTitle { get; }
        public string TitlePrefix { get; }

        #endregion Public Properties

        #region Public Methods

        public string ToLongString()
        {
            return @$"{ProductName} v{AssemblyVersion}
  Title: {ProductTitle}
  InformationalVersion: {InformationalVersion}
  FileName: {FileName}
  FilePrefix: {FilePrefix}
  File: {AssemblyPath}
  Copyright:{Copyright}
  Description: {Description}
  FileVersion: {FileVersion}
  BuildDate: {BuildDate}
  ";
        }

        public string ToShortString()
        {
            return $"{ProductTitle} v{AssemblyVersion}({BuildDate.ToString("dd.MM.yyyy")})";
        }

        public override string ToString()
        {
            return $"{ProductName} v{AssemblyVersion}({BuildDate.ToString("dd.MM.yyyy")}); _asembly: {FileName}; [{InformationalVersion}]";
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Extracts the product prefix.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private /*static*/ string ExtractProductPrefix(string fileName)
        {
            int dotIndex = fileName.IndexOf('.');
            return dotIndex > 0
                ? fileName.Substring(0, dotIndex)
                : fileName;
        }

        /// <summary>
        /// Tries the get build date.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns></returns>
        private /*static*/ DateTime? TryGetBuildDate(Version version)
        {
            if (version == null || version.Build < 0 || version.Revision < 0)
            {
                return null;
            }

            try
            {
                // .NET auto-version: Build = дни с 2000-01-01, Revision = секунды / 2
                DateTime baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime buildDate = baseDate.AddDays(version.Build).AddSeconds(version.Revision * 2);

                // Паранойя: если дата получилась из будущего, значит это не авто-версия .NET,
                // а просто какие-то числа от CI/CD системы.
                return buildDate > DateTime.UtcNow.AddDays(1) ? null : (DateTime?)buildDate;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Computes the build date.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        private DateTime ComputeBuildDate(Assembly assembly, out bool isAuto)
        {
            Version version = assembly.GetName().Version;

            // 1. Попробуем вычислить из Build/Revision
            DateTime? dt = TryGetBuildDate(version);
            if (dt.HasValue)
            {
                isAuto = true;
                return dt.Value;
            }

            // 2. Файл сборки (fallback)
            try
            {
                if (!string.IsNullOrEmpty(assembly.Location) && File.Exists(assembly.Location))
                {
                    isAuto = false;
                    return File.GetLastWriteTimeUtc(assembly.Location);
                }
            }
            catch
            {
                // ignore
            }

            // 3. Последний fallback
            isAuto = false;
            return DateTime.UtcNow;
        }

        #endregion Private Methods
    }
}