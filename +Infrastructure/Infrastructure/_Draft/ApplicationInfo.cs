using drz.Abstractions.Infrastructure;
using System;
using System.Reflection;

namespace drz.Infrastructure.Infrastructure
{
    public class ApplicationInfo : IApplicationInfo
    {
        private Assembly _assembly;

        public ApplicationInfo(Assembly assembly)
        {
            _assembly = assembly;
        }

        public string AssembleFullName
        {
            get => _assembly.FullName;
        }

        public string AssemblyPath
        {
            get => _assembly.Location;
        }

        public Version AssemblyVersion
        {
            get => _assembly.GetName().Version;
        }

        public string ProductName
        {
            get => _assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "FilePrefix";
        }

        public string TitlePrefix { get => $"{ProductName} v.{AssemblyVersion} : "; }
    }
}