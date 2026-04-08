using drz.SpecSPDS.Abstractions.Infrastructure;
using HostMgd.ApplicationServices;
using System;
using System.Reflection;

namespace drz.SpecSPDS.Infrastructure.Infrastructure
{
    internal class ApplicationInfo : IApplicationInfo
    {
        public ApplicationInfo()
        {
            _assembly = Assembly.GetExecutingAssembly();
            _handle = Application.MainWindow.Handle;
        }

        public Version Version
        {
            get => _assembly.GetName().Version!;
        }
        public string Path
        {
            get => _assembly.Location;
        }
        public string Name
        {
            get => _assembly.FullName!;
        }

        public IntPtr CadWindowHandle { get => _handle; }
        public string TitlePrefix { get => $"CadSimpleInject v.{Version} : "; }

        private Assembly _assembly;
        private IntPtr _handle;
    }
}
