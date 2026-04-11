using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using drz.Infrastructure.Infrastructure;
using drz.Infrastructure.Services;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.Reflection;

namespace drz.DiContainer
{
    public class DiRegister //: IDisposable
    {
        public DiRegister(Assembly addOnAssembly)
        //public   Container Build(Assembly addOnAssembly)
        {
            Container container = new Container();

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            RegisterInfrastructure(container, addOnAssembly);

            RegisterServices(container);

            container.Verify();

            ContainerIn = container;

            //return container;
        }

        public Container ContainerIn { get; private set; }

        private void RegisterInfrastructure(Container container, Assembly addOnAssembly)
        {
            container.RegisterInstance(addOnAssembly);

            container.Register<IApplicationInfo, ApplicationInfo>(Lifestyle.Singleton);
        }

        private void RegisterServices(Container container)
        {
            container.Register<ICommandLineMessageService, CommandLineMessageService>();

            container.Register<IWindowMessageService, WindowMessageService>(Lifestyle.Singleton);

            container.Register<IDocumentService, DocumentService>(Lifestyle.Transient);
        }
    }
}