using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace dRz.SpecSpds.Test.Tests
{
    public sealed class OsInfo
    {
        public static OsInfo Current { get; } = new OsInfo();

        public bool Is64BitOS { get; }

        public string OSDescription { get; } = string.Empty;

        public Version OSVersion { get; } = new Version();

        public string ArchitectureName { get; } = string.Empty;


        private OsInfo()
        {
            Is64BitOS = Environment.Is64BitOperatingSystem;

            ArchitectureName = Is64BitOS == true ? "x64" : "x32";

            OSDescription = Environment.OSVersion.VersionString;

            OSVersion = Environment.OSVersion.Version;

            Console.WriteLine("***************");

            System.Collections.IDictionary vs = Environment.GetEnvironmentVariables();

            foreach (System.Collections.DictionaryEntry v in vs)
            {
                Console.WriteLine($"{v.Key} {v.Value}");
            }

        }

    }
}

