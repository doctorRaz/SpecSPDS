using System;

namespace dRz.Loader.Cad.Infrastructure.Info
{
    public static class InfoOs
    {
        //public static InfoOs Current { get; } = new InfoOs();

        public static bool Is64BitOS { get; }

        public static string OsDescription { get; } = string.Empty;

        public static Version OsVersion { get; } = new Version();

        public static string OsArchitecture { get; } = string.Empty;

        static InfoOs()
        {
            Is64BitOS = Environment.Is64BitOperatingSystem;

            OsArchitecture = Is64BitOS ? "64-bit" : "32-bit";

            OsDescription = Environment.OSVersion.VersionString;

            OsVersion = Environment.OSVersion.Version;
        }

        public static string GetEnvironmentInfo()
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

