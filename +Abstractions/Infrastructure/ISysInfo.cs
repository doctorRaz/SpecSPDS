//GPT
// https://chatgpt.com/c/69c44adf-7f6c-8331-80de-c905a35fea87

using System;

namespace drz.Abstractions.Infrastructure
{
    public interface ISysInfo
    {
        //static abstract SysInfo Current { get; }
        string Architecture { get; }
        string BuildLab { get; }
        string DisplayVersion { get; }
        string EditionId { get; }
        string InstallationType { get; }
        bool IsFallback { get; }
        Version OsVersion { get; }
        string ProductName { get; }
        string VersionString { get; }

        //static abstract SysInfo Refresh();
        string ToString();
    }
}