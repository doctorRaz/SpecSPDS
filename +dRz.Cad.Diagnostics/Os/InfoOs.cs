//GPT
// https://chatgpt.com/c/69c44adf-7f6c-8331-80de-c905a35fea87

using Microsoft.Win32;
using System;

namespace dRz.Cad.Diagnostics.Os
{
    /// <summary>
    /// Информация об ОС (Registry + Environment fallback)
    /// </summary>
    public sealed class InfoOs
    {


        private const string RegPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

        public string ProductName { get; private set; }
        public string DisplayVersion { get; private set; }
        public Version OsVersion { get; private set; }
        public string VersionString { get; private set; }
        public string Architecture { get; private set; }

        // Дополнительно
        public string EditionId { get; private set; }
        public string InstallationType { get; private set; }
        public string BuildLab { get; private set; }

        /// <summary>
        /// true = использован fallback (Environment)
        /// </summary>
        public bool IsFallback { get; private set; }

        private InfoOs()
        {
            try
            {
                using (RegistryKey key = OpenKey())
                {
                    if (key == null)
                    {
                        FillFromEnvironment();
                        return;
                    }

                    ProductName = GetString(key, "ProductName", "Unknown");

                    DisplayVersion =
                        GetString(key, "DisplayVersion", null)
                        ?? GetString(key, "ReleaseId", null)
                        ?? "Unknown";

                    int major = GetInt(key, "CurrentMajorVersionNumber");
                    int minor = GetInt(key, "CurrentMinorVersionNumber");

                    if (major == 0)
                    {
                        ParseLegacyVersion(GetString(key, "CurrentVersion", null), out major, out minor);
                    }

                    int build = ParseInt(GetString(key, "CurrentBuild", null));
                    int rev = GetInt(key, "UBR");

                    OsVersion = new Version(major, minor, build, rev);
                    VersionString = OsVersion.ToString();

                    Architecture = Environment.Is64BitOperatingSystem ? "X64" : "X32";

                    EditionId = GetString(key, "EditionID", "Unknown");
                    InstallationType = GetString(key, "InstallationType", "Unknown");
                    BuildLab = GetString(key, "BuildLab", "Unknown");

                    IsFallback = false;
                }
            }
            catch
            {
                FillFromEnvironment();
            }
        }

        private void FillFromEnvironment()
        {
            try
            {
                OperatingSystem os = Environment.OSVersion;

                ProductName = "Windows";
                DisplayVersion = "Unknown";

                Version v = os.Version;
                OsVersion = new Version(v.Major, v.Minor, v.Build, v.Revision);
                VersionString = OsVersion.ToString();

                Architecture = Environment.Is64BitOperatingSystem ? "X64" : "X32";

                EditionId = "Unknown";
                InstallationType = "Unknown";
                BuildLab = "Unknown";

                IsFallback = true;
            }
            catch
            {
                ProductName = "Unknown";
                DisplayVersion = "Unknown";
                OsVersion = new Version(0, 0, 0, 0);
                VersionString = OsVersion.ToString();
                Architecture = "Unknown";
                EditionId = "Unknown";
                InstallationType = "Unknown";
                BuildLab = "Unknown";

                IsFallback = true;
            }
        }

        private static RegistryKey OpenKey()
        {
            try
            {
                RegistryKey baseKey = RegistryKey.OpenBaseKey(
                    RegistryHive.LocalMachine,
                    Environment.Is64BitOperatingSystem
                        ? RegistryView.Registry64
                        : RegistryView.Registry32);

                return baseKey.OpenSubKey(RegPath);
            }
            catch
            {
                return null;
            }
        }

        private static string GetString(RegistryKey key, string name, string fallback)
        {
            try
            {
                if (key == null)
                {
                    return fallback;
                }

                object value = key.GetValue(name);
                return value != null ? value.ToString() : fallback;
            }
            catch
            {
                return fallback;
            }
        }

        private static int GetInt(RegistryKey key, string name)
        {
            try
            {
                if (key == null)
                {
                    return 0;
                }

                object value = key.GetValue(name);
                return value is int ? (int)value : 0;
            }
            catch
            {
                return 0;
            }
        }

        private static int ParseInt(string value)
        {
            int result;
            return int.TryParse(value, out result) ? result : 0;
        }

        private static void ParseLegacyVersion(string value, out int major, out int minor)
        {
            major = 0;
            minor = 0;

            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            string[] parts = value.Split('.');
            if (parts.Length >= 2)
            {
                int.TryParse(parts[0], out major);
                int.TryParse(parts[1], out minor);
            }
        }

        private static readonly Lazy<InfoOs> _current = new Lazy<InfoOs>(() => new InfoOs());


        public static InfoOs Current
        {
            get { return _current.Value; }
        }

        public static InfoOs Refresh()
        {
            return new InfoOs();
        }


        public override string ToString()
        {
            return fullString;
        }

        private string fullString => string.Format("{0} {1} {2} ({3}) [{4}]",
                   IsFallback ? "OS (fallback):" : "OS:",
                   ProductName,
                   DisplayVersion,
                   VersionString,
                   Architecture);



    }
}