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

