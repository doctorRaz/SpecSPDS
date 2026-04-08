using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace drz.Cad.Diagnostics.AddOn;

/// <summary>
/// Runtime environment for specific assembly (addon/module).
/// Immutable per-assembly context.
/// </summary>
public sealed class InfoAddOn
{
    private const string _nLogConfigFileName = "drzNLog.dll.nlog";

    private static readonly ConcurrentDictionary<Assembly, InfoAddOn> _cache = new();

    /// <summary>
    /// Gets the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns></returns>
    public static InfoAddOn Get(Assembly assembly)
    {
        return _cache.GetOrAdd(assembly, asm => new InfoAddOn(asm));
    }

    public static InfoAddOn Get(Type type)
    {
        return Get(type.Assembly);
    }

    private InfoAddOn(Assembly assembly)
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

        FileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version
            ?? "Unknown";

        Copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright
            ?? "Unknown";

        Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description
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
    public static InfoAddOn FromAssembly(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        return new InfoAddOn(assembly);
    }

    /// <summary>
    /// Froms the type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    public static InfoAddOn FromType(Type type)
        => new InfoAddOn(type.Assembly);

    /// <summary>
    /// НЕ рекомендуется для production (может вернуть не ту сборку)
    /// </summary>
    public static InfoAddOn FromCallingAssembly()
        => new InfoAddOn(Assembly.GetCallingAssembly());

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
    /// Gets the build date.
    /// </summary>
    /// <value>
    /// The build date.
    /// </value>
    public DateTime BuildDate { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is automatic version.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is automatic version; otherwise, <c>false</c>.
    /// </value>
    public bool IsAutoVersion { get; }

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
        return $"{ProductName} v{AssemblyVersion}({BuildDate.ToString("dd.MM.yyyy")}); Assembly: {FileName}; [{InformationalVersion}]";
    }

    public string ToShortString()
    {
        return $"{ProductTitle} v{AssemblyVersion}({BuildDate.ToString("dd.MM.yyyy")})";
    }
    /// <summary>
    /// Converts Longs the string.
    /// </summary>
    /// <returns></returns>
    public string ToStringLong()
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

    // -------------------- Helpers --------------------


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