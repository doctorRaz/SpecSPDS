using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using drz.CadServices.Services;
using drz.Infrastructure.Infrastructure;
using drz.Infrastructure.Services;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Reflection;

namespace drz.AddOn.Composition
{
    public class AddOnCompositionRoot : IDisposable
    {
        private readonly Container _container;

        private bool _disposed;

        public AddOnCompositionRoot(Assembly addOnAssembly)
        {
            _container = new Container();

            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            RegisterInfrastructure(_container, addOnAssembly);

            RegisterServices(_container);

            _container.Verify();
        }

        /// <summary>
        /// Получить сервис
        /// </summary>
        public TService Get<TService>() where TService : class
        {
            return _container.GetInstance<TService>();
        }

        /// <summary>
        /// Создать скоуп для Scoped-сервисов
        /// </summary>
        public Scope BeginScope() => AsyncScopedLifestyle.BeginScope(_container);

        private void RegisterInfrastructure(Container container, Assembly addOnAssembly)
        {
            container.RegisterInstance(addOnAssembly);

            container.Register<IApplicationInfo, ApplicationInfo>(Lifestyle.Singleton);

            container.Register<IApplicationInfo_NEW, ApplicationInfo_NEW>(Lifestyle.Singleton);

            container.Register<ISysInfo, SysInfo_NEW>(Lifestyle.Singleton);
        }

        private void RegisterServices(Container container)
        {
            container.Register<IWindowHandleProvider, CadWindowProvider>(Lifestyle.Singleton);//IntPtr Handle

            container.Register<ICommandLineMessageService, CommandLineMessageService>();

            container.Register<IWindowMessageService, WindowMessageService>(Lifestyle.Singleton);

            container.Register<IDocumentService, DocumentService>(Lifestyle.Singleton);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _container.Dispose();
                _disposed = true;
            }
        }
    }
}