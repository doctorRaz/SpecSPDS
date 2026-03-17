using System;
using System.Diagnostics;
using System.IO;

namespace dRz.SpecSpds.Test.Tests
{
    /// <summary>
    /// Максимально полная информация о процессе-хосте (nanoCAD)
    /// </summary>
    public sealed class CadHostInfo
    {

        public static CadHostInfo Current { get; } = new CadHostInfo();

        public string ExePath { get; } = string.Empty;

        public string InstallDirectory { get; } = string.Empty;

        public string FileName { get; } = string.Empty;

        public Version ProductVersion { get; } = new Version();

        public string FileVersion { get; } = string.Empty;

        public string ProductName { get; } = string.Empty;

        public string CompanyName { get; } = string.Empty;

        public string FileDescription { get; } = string.Empty;

        public string OriginalFilename { get; } = string.Empty;

        public string Copyright { get; } = string.Empty;

        public bool Is64BitProcess { get; }

        public string ArchitectureName { get; } = string.Empty;


        private CadHostInfo()
        {
            try
            {
                //ExePath = HostExePath.GetExePath();
                ExePath = CadHostExePath.GetExePathProcess();

#if !(NC || NC26)
                ExePath = @"c:\Program Files\Nanosoft\nanoCAD x64 26.0\nCadM.exe";
#endif

                if (!string.IsNullOrEmpty(ExePath))
                {
                    FileInfo fi = new FileInfo(ExePath);

                    InstallDirectory = fi.DirectoryName!;

                    FileName = fi.Name;


                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(ExePath);

                    #region Version
                    
                    FileVersion = fvi.FileVersion!;

                    int major = fvi.ProductMajorPart;
                    int minor = fvi.ProductMinorPart;
                    int build = fvi.ProductBuildPart;
                    int privatePart = fvi.ProductPrivatePart;

                    ProductVersion = new Version(major, minor, build, privatePart);

                    #endregion

                    ProductName = fvi.ProductName!;

                    CompanyName = fvi.CompanyName!;

                    FileDescription = fvi.FileDescription!;

                    OriginalFilename = fvi.OriginalFilename!;

                    Copyright = fvi.LegalCopyright!;

                    Is64BitProcess = Environment.Is64BitProcess;

                    ArchitectureName = Is64BitProcess==true?"x64":"x32" ;
                }
            }
            catch
            {
                // intentionally ignore
            }
        }
    }
}


/*
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace dRz.SpecSpds.Test.Tests
{
    /// <summary>
    /// Информация о процессе-хосте (nanoCAD)
    /// </summary>
    public sealed class CadHostInfo
    {
        private static readonly Lazy<CadHostInfo> _current = new Lazy<CadHostInfo>(() => new CadHostInfo());

        public static CadHostInfo Current => _current.Value;

        public int ProcessId { get; }

        public string ExePath { get; } = string.Empty;
        public string InstallDirectory { get; } = string.Empty;
        public string FileName { get; } = string.Empty;

        public string FileVersion { get; } = string.Empty;
        public string ProductVersion { get; } = string.Empty;

        public string ProductName { get; } = string.Empty;
        public string CompanyName { get; } = string.Empty;
        public string FileDescription { get; } = string.Empty;

        public string OriginalFilename { get; } = string.Empty;
        public string Copyright { get; } = string.Empty;

        public bool Is64BitProcess { get; }
        public string ProcessArchitecture { get; }

        public Exception Error { get; }

        private CadHostInfo()
        {
            try
            {
                var process = Process.GetCurrentProcess();

                ProcessId = process.Id;

                Is64BitProcess = Environment.Is64BitProcess;
                ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString();

                string exePath = null;

                try
                {
                    exePath = process.MainModule?.FileName;
                }
                catch
                {
                    // иногда может бросить Win32Exception
                }

                if (string.IsNullOrEmpty(exePath))
                    return;

                ExePath = exePath;

                var fi = new FileInfo(exePath);

                InstallDirectory = fi.DirectoryName ?? string.Empty;
                FileName = fi.Name;

                var fvi = FileVersionInfo.GetVersionInfo(exePath);

                FileVersion = fvi.FileVersion ?? string.Empty;
                ProductVersion = fvi.ProductVersion ?? string.Empty;

                ProductName = fvi.ProductName ?? string.Empty;
                CompanyName = fvi.CompanyName ?? string.Empty;
                FileDescription = fvi.FileDescription ?? string.Empty;

                OriginalFilename = fvi.OriginalFilename ?? string.Empty;

                Copyright = fvi.LegalCopyright ?? string.Empty;
            }
            catch (Exception ex)
            {
                Error = ex;
            }
        }

        /// <summary>
        /// Диагностическая строка
        /// </summary>
        public string ToDiagnosticString()
        {
            return
$@"Host process info
------------------------

ProcessId: {ProcessId}

ExePath: {ExePath}
InstallDirectory: {InstallDirectory}
FileName: {FileName}

Architecture:
  Process: {ProcessArchitecture}
  Is64Bit: {Is64BitProcess}

FileVersionInfo:
  FileVersion: {FileVersion}
  ProductVersion: {ProductVersion}
  ProductName: {ProductName}
  CompanyName: {CompanyName}
  Description: {FileDescription}
  OriginalFilename: {OriginalFilename}

Copyright: {Copyright}

Error: {Error}";
        }
    }
}

*/