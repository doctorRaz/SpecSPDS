using drz.Abstractions.Infrastructure;
using System;
using System.IO;
using System.Reflection;

namespace drz.Infrastructure.Infrastructure
{
    //00:00:00.0056869 ApplicationInfo_NEW
    public class AddOnInfo : IAddOnInfo
    {
        #region Private Fields

        private readonly Assembly _assembly;

        #endregion Private Fields

        #region Public Constructors

        private DateTime? _buildDate;

        private string? _copyright;

        private string? _description;

        private string? _fileVersion;

        private string? _informationalVersion;

        private bool _isAutoVersion;

        private string? _productTitle;

        public AddOnInfo(Assembly assembly)
        {
            _assembly =
                assembly ?? throw new ArgumentNullException(nameof(assembly));

            // 1. Базовые данные о путях (работа со строками — это быстро)
            AssemblyPath = _assembly.Location ?? string.Empty;

            AssembleFullName = _assembly.FullName;

            AssemblyName assemblyName = _assembly.GetName();

            if (!string.IsNullOrEmpty(AssemblyPath))
            {
                AssemblyDirectory =
                    Path.GetDirectoryName(AssemblyPath) ?? string.Empty;

                FileName =
                    Path.GetFileNameWithoutExtension(AssemblyPath);
            }
            else
            {
                AssemblyDirectory = string.Empty;

                FileName =
                    assemblyName.Name ?? "Unknown";
            }

            FilePrefix = ExtractProductPrefix(FileName);

            // 2. Данные версии (GetName тоже относительно быстр, но вызываем 1 раз)
            AssemblyVersion =
                assemblyName.Version ?? new Version(0, 0, 0, 0);

            // 3. Подготовка путей AppData (без Reflection)
            // ---AppData ---
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (string.IsNullOrEmpty(appData))
            {
                appData = Path.GetTempPath(); // fallback
            }

            // Внимание: ProductName здесь берется лениво ниже,
            // поэтому для путей используем FilePrefix или вычисляем ProductName сразу, если он критичен.
            // Но лучше ProductName вычислить в конструкторе, так как он нужен для путей:
            ProductName =
                assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? FilePrefix;

            AppDataProductPath = Path.Combine(appData, ProductName);

            AppDataProductLogPath = Path.Combine(AppDataProductPath, "Logs");

            TitlePrefix = $"{ProductName} v.{AssemblyVersion} : ";


            RootPath =;//
        }
        public DateTime BuildDate => _buildDate ??= ComputeBuildDate(_assembly, out _isAutoVersion);

        public string Copyright => _copyright ??=
               _assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "Unknown";

        public string Description => _description ??=
               _assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "Unknown";

        public string FileVersion => _fileVersion ??=
                   _assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "Unknown";

        public string InformationalVersion => _informationalVersion ??=
                  _assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                  ?? "Unknown";

        public bool IsAutoVersion
        {
            get
            {
                if (_buildDate == null) { var _ = BuildDate; } // Триггерим вычисление даты
                return _isAutoVersion;
            }
        }
        public string ProductTitle => _productTitle ??=
            _assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? FileName;
        #endregion Public Constructors

        #region Public Properties

        public string AppDataProductLogPath { get; }
        public string AppDataProductPath { get; }
        public string AssembleFullName { get; }
        public string AssemblyDirectory { get; }
        public string AssemblyPath { get; }
        public Version AssemblyVersion { get; }
        public string FileName { get; }
        public string FilePrefix { get; }
        public string ProductName { get; }
        public string TitlePrefix { get; }
        public string RootPath { get; }

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
            return $"{ProductName} v{AssemblyVersion}({BuildDate.ToString("dd.MM.yyyy")}); assembly: {FileName}; [{InformationalVersion}]";
        }

        #endregion Public Methods

        #region Private Methods

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

        /// <summary>
        /// Extracts the product prefix.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private string ExtractProductPrefix(string fileName)
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
        private DateTime? TryGetBuildDate(Version version)
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
                return buildDate > DateTime.UtcNow.AddDays(1) ? null : buildDate;
            }
            catch
            {
                return null;
            }
        }

        #endregion Private Methods
    }
}