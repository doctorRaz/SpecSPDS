using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace dRz.SpecSpds.Test.Tests
{
    /// <summary>
    /// Максимально полная информация о процессе-хосте (nanoCAD)
    /// </summary>
    public sealed class HostInfo
    {

        public static HostInfo Current { get; } = new HostInfo();

        public string ExecutablePath { get; }
        public string ExecutableDirectory { get; }
        public string FileName { get; }

        public string FileVersion { get; }

        public string ProductVersion { get; }

        public string ProductName { get; }
        public string CompanyName { get; }
        public string FileDescription { get; }

        public string OriginalFilename { get; }

        public string Copyright { get; }

     

        private HostInfo()
        {
            try
            {
                Process p = Process.GetCurrentProcess();


                ExecutablePath = p.MainModule?.FileName ?? "";

                if (!string.IsNullOrEmpty(ExecutablePath))
                {
                    FileInfo fi = new FileInfo(ExecutablePath);

                    ExecutableDirectory = fi.DirectoryName;
                    FileName = fi.Name;

                }


                if (!string.IsNullOrEmpty(ExecutablePath))
                {
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(ExecutablePath);

                    FileVersion = fvi.FileVersion;
                    ProductVersion = fvi.ProductVersion;

                    ProductName = fvi.ProductName;
                    CompanyName = fvi.CompanyName;
                    FileDescription = fvi.FileDescription;

                    OriginalFilename = fvi.OriginalFilename;

                    Copyright = fvi.LegalCopyright;

           
                }
            }
            catch
            {
                // intentionally ignore
            }
        }

        /// <summary>
        /// Строка со всей диагностической информацией
        /// </summary>
        public string ToDiagnosticString()
        {
            return
$@"Host process info
------------------------

ExecutablePath: {ExecutablePath}
ExecutableDirectory: {ExecutableDirectory}
FileName: {FileName}

FileVersionInfo:
  FileVersion: {FileVersion}
  ProductVersion: {ProductVersion}
  ProductName: {ProductName}
  CompanyName: {CompanyName}
  Description: {FileDescription}
  OriginalFilename: {OriginalFilename}

  Copyright: {Copyright}";
        }
    }
}