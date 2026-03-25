using Microsoft.Win32;
using System;

namespace dRz.CAD.Runtime.Info
{
    /// <summary>
    /// Информация о системме
    /// </summary>
    public sealed class InfoOs
    {
        public string ProductName { get; }
        public string DisplayVersion { get; }
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
            DisplayVersion = key?.GetValue("DisplayVersion")?.ToString() ?? "Unknown";

            Version = $"{major}.{minor}.{build}.{rev}";

            bool Is64BitOS = Environment.Is64BitOperatingSystem;

            Architecture = Is64BitOS ? "64-bit" : "32-bit";

        }

        /// <summary>
        /// Gets the current OS.
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        public static InfoOs Current { get; } = new InfoOs();

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $@"{ProductName}, {DisplayVersion}, {Version}; {Architecture}";
        }
    }

}

