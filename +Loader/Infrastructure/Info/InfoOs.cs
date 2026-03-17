using System;

namespace dRz.Loader.Cad.Infrastructure.Info
{
    public sealed class InfoOs
    {
        public static InfoOs GetInfo { get; } = new InfoOs();

        public bool Is64BitOS { get; }

        public string OSDescription { get; } = string.Empty;

        public Version OSVersion { get; } = new Version();

        public string OSArchitecture { get; } = string.Empty;

        private InfoOs()
        {
            Is64BitOS = Environment.Is64BitOperatingSystem;

            OSArchitecture = Is64BitOS ? "64-bit" : "32-bit";

            OSDescription = Environment.OSVersion.VersionString;

            OSVersion = Environment.OSVersion.Version;
        }

        public override string ToString()
        {
            return
            $@"     OS
            Description: {OSDescription}
            Version: {OSVersion}
            OSArchitecture: {OSArchitecture} 
            Is64BitOS {(Is64BitOS ? "64-bit" : "32-bit")}";
        }
    }
}

