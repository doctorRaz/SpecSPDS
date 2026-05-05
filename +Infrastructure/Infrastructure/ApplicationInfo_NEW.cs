using drz.Abstractions.Infrastructure;
using System;
using System.Reflection;

#if !TEST
using HostMgd.ApplicationServices;
#endif

namespace drz.Infrastructure.Infrastructure
{
    public class ApplicationInfo_NEW        : IApplicationInfo_NEW
    {
        private Assembly _assembly;

        private IntPtr _handle;

        public ApplicationInfo_NEW(Assembly assembly)
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

        public string AppDataProductLogPath => throw new NotImplementedException();

        public string AppDataProductPath => throw new NotImplementedException();

        public Assembly Assembly => throw new NotImplementedException();

        public string AssemblyDirectory => throw new NotImplementedException();

        public string AssemblyPath => throw new NotImplementedException();

        public Version AssemblyVersion => throw new NotImplementedException();

        public DateTime BuildDate => throw new NotImplementedException();

        public string Copyright => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public string FileName => throw new NotImplementedException();

        public string FilePrefix => throw new NotImplementedException();

        public string FileVersion => throw new NotImplementedException();

        public string InformationalVersion => throw new NotImplementedException();

        public bool IsAutoVersion => throw new NotImplementedException();

        public string NLogConfigPath => throw new NotImplementedException();



        public string ProductTitle => throw new NotImplementedException();

        string IApplicationInfo_NEW.ToShortString()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public string ToStringLong()
        {
            throw new NotImplementedException();
        }
    }
}