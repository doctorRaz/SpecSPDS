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
    /// <summary> Наполнение SimpleInjector объектами </summary>
    /// <seealso cref="System.IDisposable" />
    public class AddOnCompositionRoot : IDisposable
    {
        private readonly Container _container;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddOnCompositionRoot"/> class.
        /// </summary>
        /// <param name="addOnAssembly">The add on assembly.</param>
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

            //container.Register<IApplicationInfo, ApplicationInfo>(Lifestyle.Singleton);

            container.Register<IApplicationInfo, ApplicationInfo>(Lifestyle.Singleton);

            container.Register<ISysInfo, SysInfo>(Lifestyle.Singleton);

            container.Register<ICadInfo, CadInfo>(Lifestyle.Singleton);
        }

        private void RegisterServices(Container container)
        {
            container.Register<IWindowHandleProvider, CadWindowProvider>(Lifestyle.Singleton);//IntPtr Handle

            container.Register<ICommandLineMessageService, CommandLineMessageService>();

            container.Register<IWindowMessageService, WindowMessageService>(Lifestyle.Singleton);

            container.Register<IDocumentService, DocumentService>(Lifestyle.Singleton);
        }

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
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