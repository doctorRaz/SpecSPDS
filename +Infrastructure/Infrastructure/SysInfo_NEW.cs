using drz.Abstractions.Infrastructure;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management; // Не забудьте добавить ссылку

namespace drz.Infrastructure.Infrastructure
{
    public class SysInfo_NEW : ISysInfo
    {
        #region Private Fields

        private const string RegPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        private string _gpuInfo;
        private string _processorName;
        private string _ramTotal ;

        #endregion Private Fields

        #region Public Constructors

        public SysInfo_NEW()
        {
            // Устанавливаем базовые значения через Environment (служит дефолтом)
            Architecture = Environment.Is64BitOperatingSystem ? "X64" : "X32";

            Version envVersion = Environment.OSVersion.Version;
            OsVersion = new Version(envVersion.Major,
                        envVersion.Minor,
                        Math.Max(0, envVersion.Build),
                        Math.Max(0, envVersion.Revision));

            IsFallback = true;

            // Пытаемся обогатить данными из реестра
            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                                                                    RegistryView.Registry64);

                using RegistryKey key = baseKey.OpenSubKey(RegPath);

                if (key != null)
                {
                    ProductName = GetString(key, "ProductName", ProductName);
                    DisplayVersion = GetString(key, "DisplayVersion") ?? GetString(key, "ReleaseId") ?? DisplayVersion;
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
            catch { }

        }

        #endregion Public Constructors

        #region Public Properties

        public string Architecture { get; init; }
        public string BuildLab { get; init; } = "Unknown";
        public string DisplayVersion { get; init; } = "Unknown";
        public string EditionId { get; init; } = "Unknown";
        public string GpuInfo => _gpuInfo ??= GetGpuData();
        public string InstallationType { get; init; } = "Unknown";
        public bool IsFallback { get; init; }
        public Version OsVersion { get; init; }
        public string ProcessorName => _processorName ??= GetWmiValue("Win32_Processor", "Name");
        public string ProductName { get; init; } = "Windows";
        public string RamTotalGb => _ramTotal ??= GetRamTotal();
        public string VersionString => OsVersion.ToString();

        #endregion Public Properties

        #region Private Properties

        private string defStr => $"{(IsFallback ? "OS (fallback):" : "OS:")} {ProductName} {DisplayVersion} ({OsVersion}) [{Architecture}]";

        private string longStr => $"CPU: {ProcessorName}\n" +
                                  $"RAM: {RamTotalGb}\n" +
                          $"GPU: {GpuInfo}";

        private string shortStr => $"{(IsFallback ? "OS (fallback):" : "OS:")} {ProductName} {DisplayVersion} [{Architecture}]";
        #endregion Private Properties

        #region Public Methods

        public string ToLongString() => $"{defStr}\n" +
                                         $"{longStr}";

        public string ToShortString() => shortStr;

        public override string ToString() => defStr;

        #endregion Public Methods

        #region Private Methods

        private string GetRamTotal()
        {
            double.TryParse(GetWmiValue("Win32_ComputerSystem", "TotalPhysicalMemory"), out double ramBytes);
            return $"{(ramBytes / (1024 * 1024 * 1024)):F1} GB";

        }
        private static string GetGpuData()
        {
            try
            {
                using ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM, VideoProcessor FROM Win32_VideoController");
                List<ManagementBaseObject> gpus = searcher.Get().Cast<ManagementBaseObject>()
                    .Where(obj =>
                        obj["VideoProcessor"] != null   // У виртуальных здесь часто null
                        ).ToList();

                if (gpus.Any())
                {
                    IEnumerable<string> results = gpus.Select(gpu =>
                    {
                        string name = gpu["Name"]?.ToString() ?? "Unknown";

                        return $"{name}".Trim();
                    });

                    return string.Join(" / ", results);
                }
            }
            catch { }
            return "Unknown";
        }

        private static int GetInt(RegistryKey key, string name, int fallback = 0) =>
                    key.GetValue(name) is int val ? val : fallback;

        private static string GetString(RegistryKey key, string name, string fallback = null) =>
                    key.GetValue(name)?.ToString() ?? fallback;

        // Новые свойства железа
        private static string GetWmiValue(string table, string property)
        {
            try
            {
                using ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {property} FROM {table}");
                foreach (ManagementBaseObject? obj in searcher.Get())
                {
                    return obj[property]?.ToString()?.Trim() ?? "Unknown";
                }
            }
            catch { }
            return "Unknown";
        }

        #endregion Private Methods
    }
}
