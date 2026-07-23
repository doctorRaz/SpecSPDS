using drz.Abstractions.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace drz.Infrastructure.Infrastructure
{
    /// <summary>
    /// AddOnInfo
    /// </summary>
    /// <seealso cref="drz.Abstractions.Infrastructure.IAddOnInfo" />
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

        /// <summary>
        /// Initializes a new instance of the <see cref="AddOnInfo"/> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <exception cref="System.ArgumentNullException">assembly</exception>
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
            RunningVersion =
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

            TitlePrefix = $"{ProductName} v.{RunningVersion} : ";

            FileInfo? package = FindPackageFile(AssemblyDirectory, ProductName);

            PackageDirectory = package?.DirectoryName ?? AssemblyDirectory;

            PackageFileName = package?.Name;

            RepositoryUrl = GetMetadata("RepositoryUrl") ?? "https://github.com/doctorRaz";

            CadFamily = GetMetadata("CadFamily") ?? "";

            CadCode = GetMetadata("CadCode") ?? "";
        }

        /// <summary>Возвращает дату-время компиляции сборки.</summary>
        /// <value>Дата-время компиляции сборки.</value>
        public DateTime BuildDate => _buildDate ??= ComputeBuildDate(_assembly, out _isAutoVersion);

        /// <summary>Возвращает информацию о копирайте.</summary>
        /// <value>Копирайт.</value>
        public string Copyright => _copyright ??=
               _assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "Unknown";

        /// <summary>Возвращает описание сборки.</summary>
        /// <value>The description.</value>
        public string Description => _description ??=
               _assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "Unknown";

        /// <summary>Возвращает AssemblyFileVersionAttribute.</summary>
        public string FileVersion => _fileVersion ??=
                   _assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "Unknown";

        /// <summary>Возвращает AssemblyInformationalVersionAttribute.</summary>
        public string InformationalVersion => _informationalVersion ??=
                  _assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                  ?? "Unknown";

        /// <summary>Признак, что дата сборки получена из версии.</summary>
        public bool IsAutoVersion
        {
            get
            {
                if (_buildDate == null) { var _ = BuildDate; } // Триггерим вычисление даты
                return _isAutoVersion;
            }
        }

        /// <summary>Возвращает AssemblyTitleAttribute.</summary>
        public string ProductTitle => _productTitle ??=
            _assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? FileName;

        #endregion Public Constructors

        /// <summary>Gets a value indicating whether this instance has package.</summary>
        /// <value>
        /// <c>true</c> if this instance has package; otherwise, <c>false</c>.
        /// </value>
        public bool HasPackage => PackageFileName != null;

        #region Public Properties

        /// <summary>Возвращает путь к журналу данных приложения.</summary>
        /// <value>Путь к журналу данных приложения.</value>
        public string AppDataProductLogPath { get; }

        /// <summary>Возвращает путь к данным приложения.</summary>
        /// <value>Путь к данным приложения.</value>
        public string AppDataProductPath { get; }

        /// <summary>
        /// "Полное Имя" сборки, используется для показа в заголовках диалогов, окон, сообщений
        /// </summary>
        /// <value>"Полное Имя" сборки.</value>
        public string? AssembleFullName { get; }

        /// <summary>Возвращает директорию сборки.</summary>
        /// <value>Директория сборки.</value>
        public string AssemblyDirectory { get; }

        /// <summary>Возвращает полный путь к сборке.</summary>
        /// <value>Полный путь к сборке.</value>
        public string AssemblyPath { get; }

        /// <summary>Возвращает версию загруженной сборки.</summary>
        /// <value>версия сборки.</value>
        public Version RunningVersion { get; }

        /// <summary>Возвращает имя файла сборки без расширения.</summary>
        /// <value>Имя файла сборки без расширения.</value>
        public string FileName { get; }

        /// <summary>Возвращает имя файла сборки до первой точки.</summary>
        public string FilePrefix { get; }

        /// <summary>Возвращает AssemblyProductAttribute.</summary>
        public string ProductName { get; }

        /// <summary>Возвращает ProductName v.RunningVersion.</summary>
        public string TitlePrefix { get; }

        /// <summary>
        /// Возвращает путь к корневому каталогу ад дона где находится package
        /// </summary>
        /// <value>путь к корневому каталогу ад дона</value>
        public string PackageDirectory { get; }

        /// <summary>Возвращает имя файла package.</summary>
        /// <value>Имя файла package.</value>
        public string? PackageFileName { get; }

        /// <summary>Gets the repository URL.</summary>
        /// <value>The repository URL.</value>
        public string? RepositoryUrl { get; }

        /// <summary>Gets the cad family.</summary>
        /// <value>The cad family.</value>
        public string? CadFamily { get; }

        /// <summary>Gets the cad code.</summary>
        /// <value>The cad code.</value>
        public string? CadCode { get; }

        /// <summary>Возвращает версию установленной сборки.</summary>
        /// <value>версия сборки.</value>
        public Version InstalledVersion => AssemblyName.GetAssemblyName(AssemblyPath).Version ?? new Version(0, 0);

        #endregion Public Properties

        #region Public Methods

        /// <summary>Converts to longstring.</summary>
        /// <returns>long string</returns>
        public string ToLongString()
        {
            return @$"{ProductName} v{RunningVersion}
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

        /// <summary>Converts to shortstring.</summary>
        /// <returns>short string</returns>
        public string ToShortString()
        {
            return $"{ProductTitle} v{RunningVersion}({BuildDate:dd.MM.yyyy})";
        }

        /// <summary>Converts to string.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{ProductName} v{RunningVersion}({BuildDate:dd.MM.yyyy}); assembly: {FileName}; [{InformationalVersion}]";
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Возвращает значение AssemblyMetadata по указанному ключу.
        /// </summary>
        private string? GetMetadata(string key)
        {
            return _assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(a => string.Equals(a.Key, key, StringComparison.OrdinalIgnoreCase))
                ?.Value;
        }

        /// <summary>Computes the build date.</summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="isAuto">if set to <c>true</c> [is automatic].</param>
        /// <returns></returns>
        private DateTime ComputeBuildDate(Assembly assembly, out bool isAuto)
        {
            Version? version = assembly.GetName().Version;

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
                DateTime baseDate = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
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

        //получаем путь к папке ROOT с аддоном, ищем в ней все пакеты
        private FileInfo? FindPackageFile(string startDirectory, string packageName)
        {
            string prefix = packageName + ".";

            DirectoryInfo current = new(startDirectory);
            DirectoryInfo? parent = current.Parent;

            foreach (DirectoryInfo? dir in new[] { parent, current })
            {
                if (dir == null)
                    continue;

                FileInfo? package = dir.EnumerateFiles("*.package",
                                                       SearchOption.TopDirectoryOnly)
                                                       .FirstOrDefault(f => f.Name.StartsWith(prefix,
                                                       StringComparison.OrdinalIgnoreCase));

                if (package != null)
                {
                    return package;
                }
            }

            // Если пакет не найден, по умолчанию считаем корнем родительский каталог
            return null;// parent?.FullName ?? current.FullName;
        }

        /*

используем FindPackageFile для поиска папки ROOT с аддоном, затем ищем в ней все *.bak и *.~* и удаляем их

string start = Path.GetDirectoryName(typeof(Updater).Assembly.Location)!;

string? root = FindPackageFile(start, "SpecSPDS");

  */

        #endregion Private Methods
    }
}