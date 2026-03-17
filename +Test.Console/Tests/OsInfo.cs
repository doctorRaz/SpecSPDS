using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace dRz.SpecSpds.Test.Tests
{
    public sealed class OsInfo
    {
        public static OsInfo Current { get; } = new OsInfo();

        public bool Is64BitOS { get; }

        public string OSDescription { get; } = string.Empty;
        public Version OSVersion { get; } = new Version();

        public string ArchitectureName { get; } = string.Empty;


        private OsInfo()
        {
            Is64BitOS = Environment.Is64BitOperatingSystem;

            ArchitectureName = Is64BitOS == true ? "x64" : "x32";

            OSDescription = Environment.OSVersion.VersionString;

            OSVersion = Environment.OSVersion.Version;
            var _OSVersion = Environment.OSVersion;


            RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            Console.WriteLine(OSDescription);
            Console.WriteLine(OSVersion);


            Console.WriteLine("***************");

            Console.WriteLine(key.GetValue("ProductName"));
            Console.WriteLine(key.GetValue("CurrentBuild"));

            Console.WriteLine("***************");

            Console.WriteLine(Environment.MachineName);       // имя компьютера
            Console.WriteLine(Environment.UserName);          // пользователь
            Console.WriteLine(Environment.OSVersion);         // версия ОС
            Console.WriteLine(Environment.Is64BitOperatingSystem);
            Console.WriteLine(Environment.Is64BitProcess);
            Console.WriteLine(Environment.ProcessorCount);    // число CPU
            Console.WriteLine(Environment.SystemDirectory);
            Console.WriteLine(Environment.CurrentDirectory);
            Console.WriteLine(Environment.UserDomainName);
            Console.WriteLine(Environment.Version);
            Console.WriteLine(Environment.WorkingSet);

            Console.WriteLine("***************");
            Console.WriteLine("***************");

            System.Collections.IDictionary vs = Environment.GetEnvironmentVariables();

            foreach (System.Collections.DictionaryEntry v in vs)
            { Console.WriteLine($"{v.Key} {v.Value}"); }

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