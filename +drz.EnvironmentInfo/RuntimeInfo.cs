using drz.EnvironmentInfo.Cad;
using drz.EnvironmentInfo.Sys;
using System;

namespace drz.EnvironmentInfo;

public sealed class RuntimeInfo
{
    private static readonly Lazy<RuntimeInfo> _current =
        new(() => new RuntimeInfo(), true);

    public static RuntimeInfo Current => _current.Value;

    public SysInfo Os { get; }
    public CadInfo Cad { get; }
    //public AppInfo AddOn { get; }

    private RuntimeInfo()
    {
        Os = SysInfo.Current;
        Cad = CadInfo.Current;
        //AddOn = AppInfo.Get();
    }

    public override string ToString()
    {
        return $"{Os}\n{Cad}";
    }
}

public static class RT
{
    public static RuntimeInfo Info => RuntimeInfo.Current;

    public static CadInfo Cad => Info.Cad;

    public static SysInfo Os => Info.Os;

    //public static AppInfo AddOn => Info.AddOn;
}