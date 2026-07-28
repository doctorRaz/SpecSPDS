using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.Abstractions.Services.Message;
using drz.Infrastructure.Infrastructure;
using drz.Infrastructure.Services.Message;
using drz.LogBootstrap;
using drz.n.Infrastructure.Services;
using drz.n.Infrastructure.Services.Message;

using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Reflection;

//using Container = SimpleInjector.Container;

namespace drz.AddOnRuntime
{
    /// <summary> Наполнение SimpleInjector объектами </summary>
    /// <seealso cref="System.IDisposable" />
    public class AddOnCompositionRoot : IDisposable
    {
        #region Private Fields

        private readonly Container _container;

        private bool _disposed;

        #endregion Private Fields

        #region Public Constructors

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

            // регистрируем собственно сам контейнер
            // всегда доступен через AddOnContext
            // можно передавать в другие сборки параметром или по сервисам /требуются интерфейсы Abstractions/
            _container.RegisterInstance<IAddOnServices>(new AddOnServices(_container));

            _container.Verify();
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Создать скоуп для Scoped-сервисов
        /// </summary>
        public Scope BeginScope() => AsyncScopedLifestyle.BeginScope(_container);

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

        /// <summary>
        /// Получить сервис
        /// </summary>
        public TService Get<TService>() where TService : class
        {
            return _container.GetInstance<TService>();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>Registers the infrastructure.</summary>
        /// <param name="container">The container.</param>
        /// <param name="addOnAssembly">The add on assembly.</param>
        private void RegisterInfrastructure(Container container, Assembly addOnAssembly)
        {
            //регистрируемая сборка
            container.RegisterInstance(addOnAssembly);

            //IAddOnInfo регистрация не нужна
            //container.Register<IAddOnInfo, AddOnInfo>(Lifestyle.Singleton);

            //инфо о системе, пока экземплярный
            //todo засунуть в статический контейнер
            container.Register<ISysInfo, SysInfo>(Lifestyle.Singleton);

            //инфо о хосте, каде, пока экземплярный
            //todo засунуть в статический контейнер
            container.Register<ICadInfo, CadInfo>(Lifestyle.Singleton);

            // AddonInfoRegistry регистрируем интерфейс создания получения IAddOnInfo
            // хранятся в ConcurrentDictionary<string, IAddOnInfo> _addons = new();
            //ключ полный путь к файлу addOnAssembly
            container.RegisterSingleton<IAddonInfoRegistry, AddonInfoRegistry>();

            //регистрация новых IAddOnInfo и получение сущ объекта по полному пути к файлу addOnAssembly
            container.RegisterSingleton<IAddOnInfo>(() =>
                                                container.GetInstance<IAddonInfoRegistry>()
                                               .Register(addOnAssembly));
        }

        /// <summary>Registers the services.</summary>
        /// <param name="container">The container.</param>
        private void RegisterServices(Container container)
        {
            container.Register<IWindowHandleProvider, CadWindowProvider>(Lifestyle.Singleton);//IntPtr Handle

            // серви сообщений ком строки
            container.Register<ICommandLineMessageService, CommandLineMessageService>(Lifestyle.Singleton);

            //сервис мультикад сообщений
            //   внутри корявая маршрутизация:
            //      если вызов без документа, то попытается отправить в мультикад окошко
            //          если неуспех выведет алерт кад
            container.Register<IMcNotificatorMessageService, McNotificatorMessageServise>();

            //сервис Win сообщений
            container.Register<IWindowMessageService, WindowMessageService>(Lifestyle.Singleton);

            //сервис маршрутизации сообщений,
            //  документ есть ->ком строка
            //  документа нет -> Win
            container.Register<IMessageService, MessageService>(Lifestyle.Singleton);

            // сервисс документов
            container.Register<IDocumentService, DocumentService>(Lifestyle.Singleton);

            // фабрика логера, одна на ProduktName
            //  при повторном создании с ттем же продуккт наме, будет использоваться эта же фабрика
            //      если даже внутри одного аддоона, но в другой сборке будет создан контейнер и имя продуккта другое,
            //      в этой сборке будет использоваться другая фабрика
            container.RegisterSingleton<IDrzLoggerFactory>(() => NLogBootstrap.GetLoggerFactory(container.GetInstance<IAddOnInfo>()));

            //запрашивает фабрику из словаря при каждом обращении
            //container.Register<IDrzLoggerFactory>(() => NLogBootstrap.GetLoggerFactory(container.GetInstance<IAddOnInfo>()), Lifestyle.Transient);
        }

        #endregion Private Methods
    }
}