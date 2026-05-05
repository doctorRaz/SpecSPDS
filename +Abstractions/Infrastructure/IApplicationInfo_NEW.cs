using System;
using System.Reflection;

namespace drz.Abstractions.Infrastructure
{
    public interface IApplicationInfo_NEW
    {
        string AppDataProductLogPath { get; }
        string AppDataProductPath { get; }
        Assembly Assembly { get; }
        string AssemblyDirectory { get; }
        string AssemblyPath { get; }
        Version AssemblyVersion { get; }
        DateTime BuildDate { get; }
        string Copyright { get; }
        string Description { get; }
        string FileName { get; }
        string FilePrefix { get; }
        string FileVersion { get; }
        string InformationalVersion { get; }
        bool IsAutoVersion { get; }
        string NLogConfigPath { get; }
        string ProductName { get; }
        string ProductTitle { get; }

        //static abstract AppInfo FromAssembly(Assembly assembly);
        //static abstract AppInfo FromCallingAssembly();
        //static abstract AppInfo FromType(Type type);
        //static abstract AppInfo Get(Assembly assembly);
        //static abstract AppInfo Get(Type type);
        string ToShortString();
        string ToString();
        string ToStringLong();
    }
}