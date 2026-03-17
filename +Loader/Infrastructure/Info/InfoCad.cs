using System;
using System.Diagnostics;
using System.IO;

namespace dRz.Loader.Cad.Infrastructure.Info
{
    /// <summary>
    /// Максимально полная информация о процессе-хосте (nanoCAD)
    /// </summary>
    public sealed class InfoCad
    {

        //public static InfoCad Current { get; } = new InfoCad();

        public static string ExePath { get; } = string.Empty;

        public static string InstallDirectory { get; } = string.Empty;

        public static string FileName { get; } = string.Empty;

        public static Version ProductVersion { get; } = new Version();

        public static string FileVersion { get; } = string.Empty;

        public static string ProductName { get; } = string.Empty;

        public static string CompanyName { get; } = string.Empty;

        public static string FileDescription { get; } = string.Empty;

        public static string OriginalFilename { get; } = string.Empty;

        public static string Copyright { get; } = string.Empty;

        public static bool Is64BitProcess { get; }

        public static string HostArchitecture { get; } = string.Empty;


        static InfoCad()
        {
            try
            {
                //ExePath = HostExePath.GetExePath();
                ExePath = CadPath.GetExePathProcess();

#if !(NC || NC26)
                ExePath = @"c:\Program Files\Nanosoft\nanoCAD x64 26.0\nCad.exe";
#endif

                if (!string.IsNullOrEmpty(ExePath))
                {
                    FileInfo fi = new FileInfo(ExePath);

                    InstallDirectory = fi.DirectoryName!;

                    FileName = fi.Name;


                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(ExePath);

                    #region Version

                    FileVersion = fvi.FileVersion!;

                    ProductVersion = new Version(
                        fvi.ProductMajorPart,
                        fvi.ProductMinorPart,
                        fvi.ProductBuildPart,
                        fvi.ProductPrivatePart);

                    #endregion

                    ProductName = fvi.ProductName!;

                    CompanyName = fvi.CompanyName!;

                    FileDescription = fvi.FileDescription!;

                    OriginalFilename = fvi.OriginalFilename!;

                    Copyright = fvi.LegalCopyright!;

                    Is64BitProcess = Environment.Is64BitProcess;

                    HostArchitecture = Is64BitProcess ? "64-bit" : "32-bit";
                }
            }
            catch
            {
                // intentionally ignore
            }
        }

        public static string GetEnvironmentInfo()
        {
            return
            $@"     CAD
            ExePath: {ExePath}
            InstallDirectory: {InstallDirectory}
            FileName: {FileName}
            ProductVersion: {ProductVersion.ToString()}
            FileVersion: {FileVersion}
            ProductName: {ProductName}
            CompanyName: {CompanyName}
            FileDescription: {FileDescription}
            OriginalFilename: {OriginalFilename}
            Copyright: {Copyright}
            HostArchitecture: {HostArchitecture}
            Is64BitProcess: {(Is64BitProcess ? "64-bit" : "32-bit")}";
        }
    }
}

