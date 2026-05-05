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

        private IntPtr _handle;

        public ApplicationInfo(Assembly assembly)
        {
            _assembly = assembly;
#if !TEST
            _handle = Application.MainWindow.Handle;
#else
            _handle = IntPtr.Zero;
#endif
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