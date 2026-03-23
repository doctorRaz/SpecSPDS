using Microsoft.Win32;
using System;

namespace dRz.Loader.Infrastructure.Info
{

    public sealed class InfoOs
    {
        public string ProductName { get; }
        public string Version { get; }
        public string Architecture { get; }

        private InfoOs()
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            ProductName = key?.GetValue("ProductName")?.ToString() ?? "Unknown";

            int major = key?.GetValue("CurrentMajorVersionNumber") as int? ?? 0;
            int minor = key?.GetValue("CurrentMinorVersionNumber") as int? ?? 0;

            if (major == 0)
            {
                var v = key?.GetValue("CurrentVersion")?.ToString() ?? "0.0";
                var parts = v.Split('.');
                if (parts.Length >= 2)
                {
                    int.TryParse(parts[0], out major);
                    int.TryParse(parts[1], out minor);
                }
            }

            int build = int.TryParse(key?.GetValue("CurrentBuild")?.ToString(), out var b) ? b : 0;
            int rev = key?.GetValue("UBR") is int ubr ? ubr : 0;

            Version = $"{major}.{minor}.{build}.{rev}";

            bool Is64BitOS = Environment.Is64BitOperatingSystem;

            Architecture = Is64BitOS ? "64-bit" : "32-bit";


        }

        public static InfoOs Current { get; } = new InfoOs();

        public override string ToString()
        {
            return
                    $@"{ProductName} Version: {Version} Architecture: {Architecture}";
        }
    }

    //x прибить?
    public static class InfoOs_
    {
        //public static InfoOs Current { get; } = new InfoOs();

        public static bool Is64BitOS { get; }

        public static string OsDescription { get; } = string.Empty;

        public static Version OsVersion { get; } = new Version();

        public static string OsArchitecture { get; } = string.Empty;

        static InfoOs_()
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

