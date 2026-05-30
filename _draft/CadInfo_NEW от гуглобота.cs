using drz.Abstractions.Infrastructure;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace drz.Infrastructure.Infrastructure;

public class CadInfo_NEW : ICadInfo
{
    // Значения по умолчанию
    public string CompanyName { get; } = string.Empty;
    public string Copyright { get; } = string.Empty;
    public string ExePath { get; } = string.Empty;
    public string FileDescription { get; } = string.Empty;
    public string FileName { get; } = string.Empty;
    public Version FileVersion { get; } = new(0, 0, 0, 0);
    public string HostArchitecture { get; }
    public string InstallDirectory { get; } = string.Empty;
    public bool Is64BitProcess { get; } = Environment.Is64BitProcess;
    public bool IsFallback { get; private set; }
    public string OriginalFilename { get; } = string.Empty;
    public string ProductName { get; } = "Unknown CAD";
    public Version ProductVersion { get; } = new(0, 0, 0, 0);

    public CadInfo_NEW()
    {
        HostArchitecture = RuntimeInformation.ProcessArchitecture.ToString();

        try
        {
            string path = GetExePath();
            if (string.IsNullOrEmpty(path)) throw new Exception("Path not found");

            ExePath = path;
            var fileInfo = new FileInfo(path);
            InstallDirectory = fileInfo.DirectoryName ?? string.Empty;
            FileName = fileInfo.Name;

            var fvi = FileVersionInfo.GetVersionInfo(path);
            
            ProductName = fvi.ProductName ?? ProductName;
            CompanyName = fvi.CompanyName ?? string.Empty;
            FileDescription = fvi.FileDescription ?? string.Empty;
            OriginalFilename = fvi.OriginalFilename ?? string.Empty;
            Copyright = fvi.LegalCopyright ?? string.Empty;

            ProductVersion = new Version(fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart, fvi.ProductPrivatePart);
            FileVersion = new Version(fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);
            
            IsFallback = false;
        }
        catch
        {
            IsFallback = true;
        }
    }

    public override string ToString() =>
        $"{(IsFallback ? "CAD (fallback):" : "CAD:")} " +
        $"{(string.IsNullOrWhiteSpace(FileDescription) ? ProductName : FileDescription)} " +
        $"{ProductVersion} ({FileVersion}) [{HostArchitecture}]";

    private static string GetExePath()
    {
        // 1. Самый быстрый способ в современном .NET
        // return Environment.ProcessPath ?? string.Empty; 

        // 2. Универсальный способ (без создания объекта Process)
        var sb = new StringBuilder(260); // MAX_PATH
        if (GetModuleFileName(IntPtr.Zero, sb, sb.Capacity) > 0)
            return sb.ToString();

        // 3. Fallback
        try { return Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty; }
        catch { return string.Empty; }
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);
}
