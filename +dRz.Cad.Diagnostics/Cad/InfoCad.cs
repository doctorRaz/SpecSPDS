using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace dRz.Cad.Diagnostics.Cad
{
    /// <summary>
    /// InfoCad
    /// </summary>
    public sealed class InfoCad
    {
        private static readonly Lazy<InfoCad> _current =
            new Lazy<InfoCad>(() => new InfoCad(), true);

        /// <summary>
        /// Gets the current.
        /// </summary>
        /// <value>
        /// The current InfoCad.
        /// </value>
        public static InfoCad Current => _current.Value;

        public string ExePath { get; }
        public string InstallDirectory { get; }
        public string FileName { get; }
        public Version ProductVersion { get; }
        public Version FileVersion { get; }
        public string ProductName { get; }
        public string CompanyName { get; }
        public string FileDescription { get; }
        public string OriginalFilename { get; }
        public string Copyright { get; }
        public bool Is64BitProcess { get; }
        public string HostArchitecture { get; }

        /// <summary>
        /// true = использован fallback (Environment)
        /// </summary>
        public bool IsFallback { get; private set; }

        private InfoCad()
        {
            try
            {
                string exePath = CadPath.GetExePath();

                ExePath = exePath ?? string.Empty;
                InstallDirectory = Safe(() => new FileInfo(exePath).DirectoryName);
                FileName = Safe(() => Path.GetFileName(exePath));

                FileVersionInfo fvi = Safe(() => FileVersionInfo.GetVersionInfo(exePath));

                ProductVersion = fvi != null
                    ? new Version(fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart, fvi.ProductPrivatePart)
                    : new Version(0, 0, 0, 0);

                FileVersion = fvi != null
                    ? new Version(fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart)
                    : new Version(0, 0, 0, 0);

                ProductName = fvi?.ProductName ?? string.Empty;
                CompanyName = fvi?.CompanyName ?? string.Empty;
                FileDescription = fvi?.FileDescription ?? string.Empty;
                OriginalFilename = fvi?.OriginalFilename ?? string.Empty;
                Copyright = fvi?.LegalCopyright ?? string.Empty;

                Is64BitProcess = Environment.Is64BitProcess;
                HostArchitecture = RuntimeInformation.ProcessArchitecture.ToString();

                IsFallback = false;
            }
            catch
            {
                // жесткий fallback
                ExePath = string.Empty;
                InstallDirectory = string.Empty;
                FileName = string.Empty;
                ProductVersion = new Version(0, 0, 0, 0);
                FileVersion = new Version(0, 0, 0, 0);
                ProductName = "Unknown CAD";
                CompanyName = string.Empty;
                FileDescription = string.Empty;
                OriginalFilename = string.Empty;
                Copyright = string.Empty;
                Is64BitProcess = Environment.Is64BitProcess;
                HostArchitecture = "Unknown";

                IsFallback = true;
            }
        }

        private static T Safe<T>(Func<T> f)
        {
            try { return f(); } catch { return default; }
        }

        /// <summary>
        /// Получение пути к исполняемому файлу хоста (exe)
        /// </summary>
        private static class CadPath
        {
            /// <summary>
            /// Получить полный путь к exe текущего процесса с fallback
            /// </summary>
            internal static string GetExePath()
            {
                // Сначала через Process
                try
                {
                    string path = Process.GetCurrentProcess().MainModule?.FileName;

                    path = @"c:\Program Files\Nanosoft\nanoCAD x64 26.0\nCads.exe";//todo заглушка
                    //path = @"c:\Program Files\Autodesk\AutoCAD 2026\acad.exe";//todo заглушка

                    if (!string.IsNullOrEmpty(path))
                    {
                        return path;
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
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {

            return fullString;
        }

        private string fullString => string.Format("{0} {1} {2} ({3}) [{4}]",
           IsFallback ? "CAD (fallback):" : "CAD:",
           string.IsNullOrWhiteSpace(FileDescription) ? ProductName : FileDescription,
           ProductVersion,
           FileVersion,
           HostArchitecture);
    }
}