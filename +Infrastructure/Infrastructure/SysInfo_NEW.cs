using drz.Abstractions.Infrastructure;
using Microsoft.Win32;
using System;

namespace drz.Infrastructure.Infrastructure
{
    /// <summary>
    /// Информация об ОС (Registry + Environment fallback)
    /// </summary>
    public class SysInfo_NEW : ISysInfo
    {
        private const string RegPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

        public SysInfo_NEW()
        {
            // Устанавливаем базовые значения через Environment (служит дефолтом)
            Architecture = Environment.Is64BitOperatingSystem ? "X64" : "X32";
            ProductName = "Windows";
            DisplayVersion = "Unknown";
            EditionId = "Unknown";
            InstallationType = "Unknown";
            BuildLab = "Unknown";

            Version envVersion = Environment.OSVersion.Version;

            OsVersion = new Version(envVersion.Major, envVersion.Minor, Math.Max(0, envVersion.Build), Math.Max(0, envVersion.Revision));

            IsFallback = true;

            // Пытаемся обогатить данными из реестра
            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using RegistryKey key = baseKey.OpenSubKey(RegPath);

                if (key != null)
                {
                    ProductName = GetString(key, "ProductName", ProductName);
                    DisplayVersion = GetString(key, "DisplayVersion") ?? GetString(key, "ReleaseId") ?? "Unknown";
                    EditionId = GetString(key, "EditionID", EditionId);
                    InstallationType = GetString(key, "InstallationType", InstallationType);
                    BuildLab = GetString(key, "BuildLab", BuildLab);

                    // Уточняем версию
                    int major = GetInt(key, "CurrentMajorVersionNumber", OsVersion.Major);
                    int minor = GetInt(key, "CurrentMinorVersionNumber", OsVersion.Minor);

                    // Если Win7/8 (CurrentMajor будет 0), пробуем парсить CurrentVersion
                    if (major == 0)
                    {
                        string cv = GetString(key, "CurrentVersion");
                        if (Version.TryParse(cv, out Version? parsed))
                        {
                            major = parsed.Major;
                            minor = parsed.Minor;
                        }
                    }

                    int build = int.TryParse(GetString(key, "CurrentBuild"), out int b) ? b : OsVersion.Build;
                    int rev = GetInt(key, "UBR", OsVersion.Revision);

                    OsVersion = new Version(major, minor, Math.Max(0, build), Math.Max(0, rev));
                    IsFallback = false;
                }
            }
            catch
            {
                // Если упал реестр, данные от Environment уже лежат в свойствах
            }
        }

        public string Architecture { get; init; }
        public string BuildLab { get; init; }
        public string DisplayVersion { get; init; }
        public string EditionId { get; init; }
        public string InstallationType { get; init; }
        public bool IsFallback { get; init; }
        public Version OsVersion { get; init; }
        public string ProductName { get; init; }
        public string VersionString => OsVersion.ToString();
        public override string ToString() =>
            $"{(IsFallback ? "OS (fallback):" : "OS:")} {ProductName} {DisplayVersion} ({OsVersion}) [{Architecture}]";

        private static int GetInt(RegistryKey key, string name, int fallback = 0) =>
            key.GetValue(name) is int val ? val : fallback;

        private static string GetString(RegistryKey key, string name, string fallback = null) =>
                    key.GetValue(name)?.ToString() ?? fallback;
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    { }
}