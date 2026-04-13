using drz.Abstractions.Infrastructure;

//using HostMgd.ApplicationServices;
using System;
using System.Reflection;

namespace drz.Infrastructure.Infrastructure
{
    public class ApplicationInfo : IApplicationInfo
    {
        public ApplicationInfo(Assembly assembly)
        {
            _assembly = assembly;// Assembly.GetExecutingAssembly();
            _handle = IntPtr.Zero;
            // new System.Windows.Interop.WindowInteropHelper(        Application.Current.MainWindow    ).Handle;
        }

        public Version Version
        {
            get => _assembly.GetName().Version;
        }

        public string Path
        {
            get => _assembly.Location;
        }

        public string Name
        {
            get => _assembly.FullName;
        }

        public string ProductName
        {
            get => _assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "FilePrefix";
        }

        public IntPtr CadWindowHandle { get => _handle; }
        public string TitlePrefix { get => $"{ProductName} v.{Version} : "; }

        private Assembly _assembly;
        private IntPtr _handle;
    }
}