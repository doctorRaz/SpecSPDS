using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace dRz.Cad.Diagnostics.Cad
{
    /// <summary>
    /// Полная информация о процессе-хосте (nanoCAD/AutoCAD) для диагностики и логирования.
    /// </summary>
    public sealed class InfoCad
    {
        // Ленивая инициализация свойств
        private static readonly Lazy<string> _exePath = new(() => CadPath.GetExePath());
        private static readonly Lazy<FileVersionInfo> _fvi = new(() =>
        {
            try
            {
                return !string.IsNullOrEmpty(_exePath.Value)
                    ? FileVersionInfo.GetVersionInfo(_exePath.Value)
                    : null;
            }
            catch
            {
                return null;
            }
        });

        public static string ExePath => _exePath.Value ?? string.Empty;

        public static string InstallDirectory => GetSafe(() => new FileInfo(ExePath).DirectoryName);

        public static string FileName => GetSafe(() => Path.GetFileName(ExePath));

        public static Version ProductVersion => GetSafe(() =>
        {
            FileVersionInfo f = _fvi.Value;
            return f != null
                ? SafeVersion(f.ProductMajorPart, f.ProductMinorPart, f.ProductBuildPart, f.ProductPrivatePart)
                : new Version(0, 0, 0, 0);
        });

        public static string FileVersion => _fvi.Value?.FileVersion ?? string.Empty;

        public static string ProductName => _fvi.Value?.ProductName ?? string.Empty;

        public static string CompanyName => _fvi.Value?.CompanyName ?? string.Empty;

        public static string FileDescription => _fvi.Value?.FileDescription ?? string.Empty;

        public static string OriginalFilename => _fvi.Value?.OriginalFilename ?? string.Empty;

        public static string Copyright => _fvi.Value?.LegalCopyright ?? string.Empty;

        public static bool Is64BitProcess => Environment.Is64BitProcess;

        public static string HostArchitecture => RuntimeInformation.ProcessArchitecture.ToString();

        public override string ToString()
        {
            string name = string.IsNullOrWhiteSpace(FileDescription) ? ProductName : FileDescription;
            return $"{name} v{ProductVersion} ({FileVersion}) [{HostArchitecture}]";
        }

        public /*override*/ static string ToStringInfo()
        {
            string name = string.IsNullOrWhiteSpace(FileDescription) ? ProductName : FileDescription;
            return $"{name} v{ProductVersion} ({FileVersion}) [{HostArchitecture}]";
        }

        // ----- Вспомогательные методы -----
        private static T GetSafe<T>(Func<T> func)
        {
            try
            {
                return func()!;
            }
            catch
            {
                return default!;
            }
        }

        private static Version SafeVersion(int major, int minor, int build, int revision)
        {
            try
            {
                return new Version(major, minor, build, revision);
            }
            catch
            {
                return new Version(0, 0, 0, 0);
            }
        }
    }

    /// <summary>
    /// Получение пути к исполняемому файлу хоста (exe)
    /// </summary>
    public static class CadPath
    {
        /// <summary>
        /// Получить полный путь к exe текущего процесса с fallback
        /// </summary>
        public static string GetExePath()
        {
            // Сначала через Process
            try
            {
                string path = Process.GetCurrentProcess().MainModule?.FileName;

                path = @"c:\Program Files\Nanosoft\nanoCAD x64 26.0\nCads.exe";//todo заглушка

                if (!string.IsNullOrEmpty(path))
                {
                    return path;
                }
            }
            catch
            {
                // fallback
            }

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
    }
}