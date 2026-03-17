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

        public static InfoCad GetInfo { get; } = new InfoCad();

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

        public string HostArchitecture { get; } = string.Empty;


        private InfoCad()
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

                    HostArchitecture = Is64BitProcess ? "64-bit" : "32-bit";
                }
            }
            catch
            {
                // intentionally ignore
            }
        }

        public override string ToString()
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

