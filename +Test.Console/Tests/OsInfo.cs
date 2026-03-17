using System;

namespace dRz.SpecSpds.Test.Tests
{
    public sealed class OsInfo
    {
        public static OsInfo GetInfo { get; } = new OsInfo();

        public bool Is64BitOS { get; }

        public string OSDescription { get; } = string.Empty;

        public Version OSVersion { get; } = new Version();

        public string OSArchitecture { get; } = string.Empty;


        private OsInfo()
        {
            Is64BitOS = Environment.Is64BitOperatingSystem;

            OSArchitecture = Is64BitOS == true ? "x64" : "x32";

            OSDescription = Environment.OSVersion.VersionString;

            OSVersion = Environment.OSVersion.Version;
        }

        public override string ToString()
        {
            return
            $@"OS
            Description: {OSDescription}
            Version: {OSVersion}
            Architecture: {OSArchitecture} 
            Is64BitOS {(Is64BitOS ? "64-bit" : "32-bit")}";
        }
    }
}

