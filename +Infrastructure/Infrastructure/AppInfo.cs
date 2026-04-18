using drz.Abstractions.Infrastructure;
using HostMgd.ApplicationServices;
using System;
using System.Reflection;

namespace drz.Infrastructure.Infrastructure
{
    public class AppInfo : IAppInfo
    {
        private Assembly _assembly;

        private IntPtr _handle;

        public AppInfo(Assembly assembly)
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

        string IAppInfo.AppDataProductLogPath => throw new NotImplementedException();

        string IAppInfo.AppDataProductPath => throw new NotImplementedException();

        Assembly IAppInfo.Assembly => throw new NotImplementedException();

        string IAppInfo.AssemblyDirectory => throw new NotImplementedException();

        string IAppInfo.AssemblyPath => throw new NotImplementedException();

        Version IAppInfo.AssemblyVersion => throw new NotImplementedException();

        DateTime IAppInfo.BuildDate => throw new NotImplementedException();

        string IAppInfo.Copyright => throw new NotImplementedException();

        string IAppInfo.Description => throw new NotImplementedException();

        string IAppInfo.FileName => throw new NotImplementedException();

        string IAppInfo.FilePrefix => throw new NotImplementedException();

        string IAppInfo.FileVersion => throw new NotImplementedException();

        string IAppInfo.InformationalVersion => throw new NotImplementedException();

        bool IAppInfo.IsAutoVersion => throw new NotImplementedException();

        string IAppInfo.NLogConfigPath => throw new NotImplementedException();

        string IAppInfo.ProductName => throw new NotImplementedException();

        string IAppInfo.ProductTitle => throw new NotImplementedException();

        string IAppInfo.ToShortString()
        {
            throw new NotImplementedException();
        }

        string IAppInfo.ToString()
        {
            throw new NotImplementedException();
        }

        string IAppInfo.ToStringLong()
        {
            throw new NotImplementedException();
        }
    }
}