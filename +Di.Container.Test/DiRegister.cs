using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.Reflection;
using Test.Infrastructure;
using Test.Services;

namespace drz.DiContainer
{
    public  class DiRegister //: IDisposable
    {

        public /*static*/ Container ContainerIn { get; private set; }


        public   DiRegister(Assembly addOnAssembly)
        //public   Container Build(Assembly addOnAssembly)
        {
            var container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            RegisterInfrastructure(container, addOnAssembly);
            RegisterServices(container);

            container.Verify();

            ContainerIn = container;

            //return container;
        }


        private   void RegisterInfrastructure(Container container, Assembly addonAssembly)
        {
            container.RegisterInstance(addonAssembly);

            container.Register<IApplicationInfo, ApplicationInfo>(Lifestyle.Singleton);
        }


        private   void RegisterServices(Container container)
        {
            container.Register<ICommandLineMessageService, CommandLineMessageService>();

            container.Register<IWindowMessageService, WindowMessageService>(Lifestyle.Singleton);

            container.Register<IDocumentService, DocumentService>(Lifestyle.Transient);
        }

        //---

        //public DiRegister(Assembly addonAssembly)
        //{
        //    Container = new Container();
        //    ConfigureContainer(Container);
        //    Container.Verify();

        //}


        //private void ConfigureContainer(Container container)
        //{
        //    container.RegisterInstance(Assembly.GetExecutingAssembly());

        //    container.Register<IApplicationInfo, ApplicationInfo>(Lifestyle.Singleton);

        //    container.Register<ICommandLineMessageService, CommandLineMessageService>();

        //    container.Register<IWindowMessageService, WindowMessageService>();

        //}

        //void Dispose()
        //{
        //    ContainerIn?.Dispose();
        //}
    }
}