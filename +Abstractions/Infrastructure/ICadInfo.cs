using System;

namespace drz.Abstractions.Infrastructure
{
    public interface ICadInfo
    {
        //static abstract CadInfo Current { get; }
        string CompanyName { get; }
        string Copyright { get; }
        string ExePath { get; }
        string FileDescription { get; }
        string FileName { get; }
        Version FileVersion { get; }
        string HostArchitecture { get; }
        string InstallDirectory { get; }
        bool Is64BitProcess { get; }
        bool IsFallback { get; }
        string OriginalFilename { get; }
        string ProductName { get; }
        Version ProductVersion { get; }

        string ToString();
    }
}