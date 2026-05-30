using drz.Abstractions.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace drz.EnvironmentInfo.App;

/// <summary>
/// Runtime environment for specific assembly (addon/module).
/// Immutable per-assembly context.
/// </summary>
public sealed class AppInfo /*: IApplicationInfo_NEW*/
{

    #region Private Fields

    //private const string _nLogConfigFileName = "drzNLog.dll.nlog";

    private static readonly ConcurrentDictionary<Assembly, AppInfo> _cache = new();

    /// <summary>
    /// Gets the assembly.
    /// </summary>
    /// <value>
    /// The assembly.
    /// </value>
    //private readonly Assembly _assembly;

    #endregion Private Fields

    #region Private Constructors

    private AppInfo(Assembly assembly)
    {
        //_assembly = assembly;

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

        // --- Nlog ---
        //NLogConfigPath = !string.IsNullOrEmpty(AssemblyDirectory)
        //    ? Path.Combine(AssemblyDirectory, _nLogConfigFileName)
        //    : _nLogConfigFileName;
    }

    #endregion Private Constructors

    #region Public Properties

    public string AppDataProductLogPath { get; }


    public string AppDataProductPath { get; }

    public string AssembleFullName { get; }
    public string AssemblyDirectory { get; }

    public string AssemblyPath { get; }

    public Version AssemblyVersion { get; }


    public DateTime BuildDate { get; }
     

    public string Copyright { get; }

    /// <summary>
    /// Gets the assembly description.
    /// </summary>
    public string Description { get; }

    public string FileName { get; }

    /// <summary>
    /// Gets the file prefix.
    /// </summary>
    /// <value>
    /// The file prefix.
    /// </value>
    public string FilePrefix { get; }

    /// <summary>
    /// Gets the assembly file version.
    /// </summary>
    public string FileVersion { get; }

    /// <summary>
    /// Gets the informational version.
    /// </summary>
    /// <value>
    /// The informational version.
    /// </value>
    public string InformationalVersion { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is automatic version.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is automatic version; otherwise, <c>false</c>.
    /// </value>
    public bool IsAutoVersion { get; }

    /// <summary>
    /// Gets the n log configuration path.
    /// </summary>
    /// <value>
    /// The n log configuration path.
    /// </value>
    //public string NLogConfigPath { get; }

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

    public string TitlePrefix { get => $"{ProductName} v.{AssemblyVersion} : "; }

    #endregion Public Properties

    #region Public Methods

    /// <summary>
    /// Froms the assembly.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">assembly</exception>
    public static AppInfo FromAssembly(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        return new AppInfo(assembly);
    }

    /// <summary>
    /// НЕ рекомендуется для production (может вернуть не ту сборку)
    /// </summary>
    public static AppInfo FromCallingAssembly()
        => new AppInfo(Assembly.GetCallingAssembly());

    // -------------------- Factory --------------------
    /// <summary>
    /// Froms the type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    public static AppInfo FromType(Type type)
        => new AppInfo(type.Assembly);

    /// <summary>
    /// Gets the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns></returns>
    public static AppInfo Get(Assembly assembly)
    {
        return _cache.GetOrAdd(assembly, asm => new AppInfo(asm));
    }

    public static AppInfo Get(Type type)
    {
        return Get(type.Assembly);
    }
    public string ToShortString()
    {
        return $"{ProductTitle} v{AssemblyVersion}({BuildDate.ToString("dd.MM.yyyy")})";
    }

    // -------------------- Public API --------------------
    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"{ProductName} v{AssemblyVersion}({BuildDate.ToString("dd.MM.yyyy")}); _asembly: {FileName}; [{InformationalVersion}]";
    }
    /// <summary>
    /// Converts Longs the string.
    /// </summary>
    /// <returns></returns>
    public string ToLongString()
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
    BuildDate: {BuildDate}
";
    }

    #endregion Public Methods

    // -------------------- Helpers --------------------

    #region Private Methods

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

    /// <summary>
    /// Tries the get build date.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <returns></returns>
    private static DateTime? TryGetBuildDate(Version version)
    {
        if (version == null || version.Build < 0 || version.Revision < 0)
        {
            return null;
        }

        try
        {
            // .NET auto-version: Build = дни с 2000-01-01, Revision = секунды / 2
            DateTime baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return baseDate
                .AddDays(version.Build)
                .AddSeconds(version.Revision * 2);
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