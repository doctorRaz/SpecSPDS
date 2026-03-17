using System;

namespace dRz.Loader.Cad.Infrastructure.Info
{
    public sealed class InfoOs
    {
        public static InfoOs Current { get; } = new InfoOs();

        public bool Is64BitOS { get; }

        public string OsDescription { get; } = string.Empty;

        public Version OsVersion { get; } = new Version();

        public string OsArchitecture { get; } = string.Empty;

        private InfoOs()
        {
            Is64BitOS = Environment.Is64BitOperatingSystem;

            OsArchitecture = Is64BitOS ? "64-bit" : "32-bit";

            OsDescription = Environment.OSVersion.VersionString;

            OsVersion = Environment.OSVersion.Version;
        }

        public override string ToString()
        {
            return
            $@"     OS
            OsDescription: {OsDescription}
            OsVersion: {OsVersion}
            OsArchitecture: {OsArchitecture} 
            Is64BitOS {(Is64BitOS ? "64-bit" : "32-bit")}";
        }
    }
}

