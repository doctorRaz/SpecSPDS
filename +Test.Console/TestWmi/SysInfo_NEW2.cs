using System;
using System.Linq;
using System.Management; // Не забудьте добавить ссылку
using Microsoft.Win32;
using drz.Abstractions.Infrastructure;

namespace drz.SpecSpds.Test.TestWmi
{
    public class SysInfo_NEW2 : ISysInfo
    {
        private const string RegPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

        public string Architecture { get; init; }
        public string BuildLab { get; init; }
        public string DisplayVersion { get; init; }
        public string EditionId { get; init; }
        public string InstallationType { get; init; }
        public bool IsFallback { get; init; }
        public Version OsVersion { get; init; }
        public string ProductName { get; init; }
        public string VersionString => OsVersion.ToString();

        // Новые свойства железа
        public string ProcessorName { get; init; }
        public string RamTotalGb { get; init; }
        public string GpuInfo { get; init; }

        public SysInfo_NEW2()
        {
            // 1. Данные ОС (как было)
            Architecture = Environment.Is64BitOperatingSystem ? "X64" : "X32";
            ProductName = "Windows";
            DisplayVersion = "Unknown";

            var envVersion = Environment.OSVersion.Version;
            OsVersion = new Version(envVersion.Major, envVersion.Minor, Math.Max(0, envVersion.Build), Math.Max(0, envVersion.Revision));
            IsFallback = true;

            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using var key = baseKey.OpenSubKey(RegPath);
                if (key != null)
                {
                    ProductName = GetString(key, "ProductName", ProductName);
                    DisplayVersion = GetString(key, "DisplayVersion") ?? GetString(key, "ReleaseId") ?? "Unknown";
                    EditionId = GetString(key, "EditionID", "Unknown");
                    InstallationType = GetString(key, "InstallationType", "Unknown");
                    BuildLab = GetString(key, "BuildLab", "Unknown");

                    int major = GetInt(key, "CurrentMajorVersionNumber", OsVersion.Major);
                    int minor = GetInt(key, "CurrentMinorVersionNumber", OsVersion.Minor);
                    int build = int.TryParse(GetString(key, "CurrentBuild"), out int b) ? b : OsVersion.Build;
                    int rev = GetInt(key, "UBR", OsVersion.Revision);

                    OsVersion = new Version(major, minor, Math.Max(0, build), Math.Max(0, rev));
                    IsFallback = false;
                }
            }
            catch { }

            // 2. Данные Железа (WMI)
            ProcessorName = GetWmiValue("Win32_Processor", "Name");

            double.TryParse(GetWmiValue("Win32_ComputerSystem", "TotalPhysicalMemory"), out double ramBytes);
            RamTotalGb = $"{ramBytes / (1024 * 1024 * 1024):F1} GB";

            GpuInfo = GetGpuData();
        }

        private static string GetWmiValue(string table, string property)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {table}");
                foreach (var obj in searcher.Get())
                {
                    return obj[property]?.ToString()?.Trim() ?? "Unknown";
                }
            }
            catch { }
            return "Unknown";
        }

        private static string GetGpuData()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM, VideoProcessor FROM Win32_VideoController");
                var gpus = searcher.Get().Cast<ManagementBaseObject>()
                    .Where(obj =>
                        obj["VideoProcessor"] != null   // У виртуальных здесь часто null
                        ).ToList();

                if (gpus.Any())
                {
                    var results = gpus.Select(gpu =>
                    {
                        string name = gpu["Name"]?.ToString() ?? "Unknown";

                        // Исправляем баг WMI с объемом памяти (uint32 может прийти как отрицательное число)
                        long.TryParse(gpu["AdapterRAM"]?.ToString(), out long vramRaw);
                        uint vramBytes = (uint)vramRaw; // Принудительно в беззнаковое

                        string vram = vramBytes > 0
                            ? $"({vramBytes / (1024 * 1024)} MB)"
                            : "";

                        return $"{name} {vram}".Trim();
                    });

                    return string.Join(" / ", results);
                }
            }
            catch { }
            return "Unknown";
        }


        public override string ToString() =>
            $"OS: {ProductName} {DisplayVersion} [{Architecture}]\n" +
            $"CPU: {ProcessorName}\n" +
            $"RAM: {RamTotalGb}\n" +
            $"GPU: {GpuInfo}";

        private static int GetInt(RegistryKey key, string name, int fallback = 0) =>
            key.GetValue(name) is int val ? val : fallback;

        private static string GetString(RegistryKey key, string name, string fallback = null) =>
            key.GetValue(name)?.ToString() ?? fallback;
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
