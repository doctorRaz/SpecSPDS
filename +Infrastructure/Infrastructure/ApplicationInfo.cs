using drz.Abstractions.Infrastructure;
using HostMgd.ApplicationServices;
using System;
using System.Reflection;

namespace drz.Infrastructure.Infrastructure
{
    public class ApplicationInfo : IApplicationInfo
    {
        private Assembly _assembly;

        private IntPtr _handle;

        public ApplicationInfo(Assembly assembly)
        {
            _assembly = assembly;

            _handle = Application.MainWindow.Handle;
        }

        public IntPtr CadWindowHandle { get => _handle; }

        public string Name
        {
            get => _assembly.FullName;
        }

        public string Path
        {
            get => _assembly.Location;
        }

        public string ProductName
        {
            get => _assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "FilePrefix";
        }

        public string TitlePrefix { get => $"{ProductName} v.{Version} : "; }

        public Version Version
        {
            get => _assembly.GetName().Version;
        }
    }
}