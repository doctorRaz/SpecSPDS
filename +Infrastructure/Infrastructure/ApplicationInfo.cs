using drz.Abstractions.Infrastructure;
using System;
using System.Reflection;

#if !TEST
using HostMgd.ApplicationServices;
#endif

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

        public string ProductName
        {
            get => _assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "FilePrefix";
        }

        public string TitlePrefix { get => $"{ProductName} v.{AssemblyVersion} : "; }

        public Version AssemblyVersion
        {
            get => _assembly.GetName().Version;
        }
    }
}