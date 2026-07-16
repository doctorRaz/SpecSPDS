using drz.Abstractions.Infrastructure;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace drz.Infrastructure.Infrastructure;

/// <summary>
/// CadInfo
/// </summary>
public class CadInfo : ICadInfo
{
    #region Public Constructors

    public CadInfo()
    {
        try
        {
            HostArchitecture = Environment.Is64BitProcess ? "X64" : "X32";// RuntimeInformation.ProcessArchitecture.ToString();

            string? path = GetExePath();

            if (string.IsNullOrEmpty(path)) throw new Exception("Path not found");

            ExePath = path;

            FileInfo fileInfo = new FileInfo(path);

            InstallDirectory = fileInfo.DirectoryName ?? InstallDirectory; // Safe(() => new FileInfo(exePath).DirectoryName);

            FileName = fileInfo.Name;// Safe(() => Path.GetFileName(exePath));

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);// Safe(() => FileVersionInfo.GetVersionInfo(exePath));

            ProductName = fvi.ProductName ?? ProductName;

            CompanyName = fvi.CompanyName ?? CompanyName;

            FileDescription = fvi.FileDescription ?? FileDescription;

            OriginalFilename = fvi.OriginalFilename ?? OriginalFilename;

            Copyright = fvi.LegalCopyright ?? Copyright;

            ProductVersion = new Version(fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart, fvi.ProductPrivatePart);

            FileVersion = new Version(fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);

            IsFallback = false;
        }
        catch
        {
            IsFallback = true;
        }
    }

    #endregion Public Constructors

    #region Public Properties

    // Значения по умолчанию
    public string CompanyName { get; } = string.Empty;

    public string Copyright { get; } = string.Empty;
    public string ExePath { get; } = string.Empty;
    public string FileDescription { get; } = string.Empty;
    public string FileName { get; } = string.Empty;
    public Version FileVersion { get; } = new(0, 0, 0, 0);
    public string HostArchitecture { get; } = string.Empty;
    public string InstallDirectory { get; } = string.Empty;
    public bool Is64BitProcess { get; } = Environment.Is64BitProcess;
    public bool IsFallback { get; private set; }
    public string OriginalFilename { get; } = string.Empty;
    public string ProductName { get; } = "Unknown CAD";
    public Version ProductVersion { get; } = new(0, 0, 0, 0);

    #endregion Public Properties

    #region Private Properties

    private string defString => 
       $"{(IsFallback ? "CAD (fallback):" : "CAD:")}"+
       $"{(string.IsNullOrWhiteSpace(FileDescription) ? ProductName : FileDescription)}"+
       $"{ProductVersion} {FileVersion} [{HostArchitecture}]";

    private string longString => 
       $"{(IsFallback ? "CAD (fallback):" : "CAD:")}"+
       $"{(string.IsNullOrWhiteSpace(FileDescription) ? ProductName : FileDescription)}"+
       $"{ProductVersion} {FileVersion} [{HostArchitecture}] CompanyName:{CompanyName} Copyright:{Copyright} ProductName:{ProductName}";


    private string shortString =>
       $"{(IsFallback ? "CAD (fallback):" : "CAD:")}" +
       $"{(string.IsNullOrWhiteSpace(FileDescription) ? ProductName : FileDescription)}" +
       $"{ProductVersion} [{HostArchitecture}]";

    #endregion Private Properties

    #region Public Methods

    public string ToLongString()
    {
        return longString;
    }

    public string ToShortString()
    {
        return shortString;
    }

    public override string ToString()
    {
        return defString;
    }

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// Получить полный путь к exe текущего процесса с fallback
    /// </summary>
    private static string? GetExePath()
    {
        // Сначала через Process
        try
        {
            string? path = Process.GetCurrentProcess().MainModule?.FileName;

            //path = @"c:\Program Files\Nanosoft\nanoCAD x64 26.0\nCads.exe";//todo заглушка
            //path = @"c:\Program Files\Autodesk\AutoCAD 2026\acad.exe";//todo заглушка

            if (!string.IsNullOrEmpty(path))
            {
                return path!; // подавляем предупреждение — path точно не null тут
            }
        }
        catch { } // fallback

        // Через нативный GetModuleFileName
        try
        {
            StringBuilder sb = new(1024);
            return GetModuleFileName(IntPtr.Zero, sb, sb.Capacity) > 0 ? sb.ToString() : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);

    #endregion Private Methods

}