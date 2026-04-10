using Abstractions.Factories;
using Abstractions.Infrastructure;
using Abstractions.Services;
using SimpleInjector;
using System;
using System.Reflection;
using Test.Factories;
using Test.Infrastructure;
using Test.Services;

namespace Test
{
    public class CadPlugin : IDisposable/*: Rtm.IExtensionApplication*/
    {

        private Assembly _asm;

        public CadPlugin(Assembly asm)
        {
            _asm = asm;

            //если нет библиотек или еще какой косяк
            try
            {

                Container = new Container();
                ConfigureContainer(Container);
                Container.Verify();
            }
            catch { throw; }//падаем, продолжать смысла нет

        }


        public static Container Container { get; private set; }

        private void ConfigureContainer(Container container)
        {
            container.RegisterInstance<Assembly>(_asm);

            container.Register<IApplicationInfo, ApplicationInfo>(Lifestyle.Singleton);

            container.Register<IMessageService>(() =>
            {
                IApplicationInfo applicationInfo = container.GetInstance<IApplicationInfo>();
                return new WindowMessageService(applicationInfo);
            },
                Lifestyle.Singleton);

            container.Register<WindowMessageService>(Lifestyle.Singleton);

            container.Register<CommandLineMessageService>(Lifestyle.Transient);

            container.RegisterSingleton<IMessageServiceFactory, MessageServiceFactory>();

            container.Register<IDocumentService, DocumentService>(Lifestyle.Transient);
        }

        public void Dispose()
        {
            Container.Dispose();
        }
    }
}
