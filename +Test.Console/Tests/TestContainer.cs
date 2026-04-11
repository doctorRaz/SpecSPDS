using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using drz.AddOn.Composition;
using drz.Cad.Diagnostics.Cad;
using drz.Cad.Diagnostics.Os;
using drz.Lib_A;
using System;
using static drz.Loader.Infrastructure.AddOnContext;
using /*static*/ AC = drz.Loader.Infrastructure.AddOnContext;

namespace drz.SpecSpds.Test.Tests
{
    internal class TestContainer
    {
        private static bool _initialized;

        public TestContainer()
        {
            if (_initialized) return;

            AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(TestContainer).Assembly);

            AC.Initialize(root);

            _initialized = true;
        }

        public void test()
        {
            InfoOs os = InfoOs.Current;

            Console.WriteLine(os);

            string path = InfoDll.AppDataProductLogPath;

            Console.WriteLine(InfoCad.Current);
        }

        public void TestCondole()
        {
            IApplicationInfo app = AC.Get<IApplicationInfo>();
            IMessageService messageService = AC.Get<ICommandLineMessageService>();
            messageService.ConsoleMessage($"{app.TitlePrefix}  Console message");

            messageService = AC.Get<IWindowMessageService>();
            messageService.InfoMessage("Info message");
        }
    }
}