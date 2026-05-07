using drz.Abstractions.Infrastructure;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management; // Не забудьте добавить ссылку

namespace drz.SpecSpds.Test.TestWmi
{
    public class SysInfo_NEW2 : ISysInfo
    {

        #region Private Fields

        private const string RegPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        private readonly Lazy<string> _gpuInfo;
        private readonly Lazy<string> _processorName;
        private readonly Lazy<string> _ramTotal;

        #endregion Private Fields

        #region Public Constructors

        public SysInfo_NEW2()
        {
            // Устанавливаем базовые значения через Environment (служит дефолтом)
            Architecture = Environment.Is64BitOperatingSystem ? "X64" : "X32";
            //ProductName = "Windows";
            //DisplayVersion = "Unknown";
            //EditionId = "Unknown";
            //InstallationType = "Unknown";
            //BuildLab = "Unknown";

            Version envVersion = Environment.OSVersion.Version;
            OsVersion = new Version(envVersion.Major, envVersion.Minor, Math.Max(0, envVersion.Build), Math.Max(0, envVersion.Revision));

            IsFallback = true;

            try
            {
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using RegistryKey key = baseKey.OpenSubKey(RegPath);
              
                if (key != null)
                {
                    ProductName = GetString(key, "ProductName", ProductName);
                    DisplayVersion = GetString(key, "DisplayVersion") ?? GetString(key, "ReleaseId") ?? DisplayVersion;
                    EditionId = GetString(key, "EditionID", EditionId);
                    InstallationType = GetString(key, "InstallationType", InstallationType);
                    BuildLab = GetString(key, "BuildLab", BuildLab);

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

            // Данные Железа (WMI)
            // Инициализируем "обещания" получить данные
            _processorName = new Lazy<string>(() => GetWmiValue("Win32_Processor", "Name"));
                        
            _ramTotal = new Lazy<string>(() =>
            {
                double.TryParse(GetWmiValue("Win32_ComputerSystem", "TotalPhysicalMemory"), out double ramBytes);
                return $"{(ramBytes / (1024 * 1024 * 1024)):F1} GB";
            });

            _gpuInfo = new Lazy<string>(GetGpuData);
        }

        #endregion Public Constructors

        #region Public Properties

        public string Architecture { get; init; }
        public string BuildLab { get; init; } = "Unknown";
        public string DisplayVersion { get; init; }= "Unknown";
        public string EditionId { get; init; }= "Unknown";
        public string GpuInfo => _gpuInfo.Value;
        public string InstallationType { get; init; } = "Unknown";
        public bool IsFallback { get; init; }
        public Version OsVersion { get; init; }
        public string ProcessorName => _processorName.Value;
        public string ProductName { get; init; }="Windows";
        public string RamTotalGb => _ramTotal.Value;
        public string VersionString => OsVersion.ToString();

        #endregion Public Properties

        #region Public Methods

        public string ToLongString()
        {
            throw new NotImplementedException();
        }

        public string ToShortString()
        {
            throw new NotImplementedException();
        }

        public override string ToString() =>
                                    $"OS: {ProductName} {DisplayVersion} [{Architecture}]\n" +
                    $"CPU: {ProcessorName}\n" +
                    $"RAM: {RamTotalGb}\n" +
                    $"GPU: {GpuInfo}";

        #endregion Public Methods

        #region Private Methods

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

                        // Исправляем баг WMI с объемом памяти (uint32 может прийти как отрицательное число)
                        //todo  с памятью GPU трындеж
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

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    { }
}