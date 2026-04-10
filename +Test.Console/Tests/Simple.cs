using Abstractions.Factories;
using Abstractions.Infrastructure;
using SimpleInjector;
using Test.Factories;
using Test.Infrastructure;
using Test.Services;

namespace drz.SpecSpds.Test.Tests
{
    internal class Simple 
    {

        public static Container Container { get; private set; }
        internal void Init()
        {

            Container = new Container();
            ConfigureContainer(Container);
            Container.Verify();
        }

        private void ConfigureContainer(Container container)
        {
            container.Register<IApplicationInfo, ApplicationInfo>(Lifestyle.Singleton);

            container.Register<WindowMessageService>(Lifestyle.Singleton);

            container.Register<CommandLineMessageService>(Lifestyle.Transient);

            container.RegisterSingleton<IMessageServiceFactory, MessageServiceFactory>();

        }

        public void Terminate()
        {
            Container?.Dispose();
        }

        public void Dispose()
        {
            
            Container?.Dispose();
        }
    }
}
