using drz.AddOn.Composition;
using drz.EnvironmentInfo.Cad;
using drz.EnvironmentInfo.Sys;
using System;
using static drz.Src.Infrastructure.AddOnContext;

using AC = drz.Src.Infrastructure.AddOnContext;

namespace drz.SpecSpds.Test.SimpleInjector
{
    internal class TestContainer
    {
        private static bool _initialized;

        public TestContainer()
        {
            if (_initialized)
            {
                return;
            }

            AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(TestContainer).Assembly);

            AC.Initialize(root);

            _initialized = true;
        }

        public void test()
        {
            SysInfo os = SysInfo.Current;

            Console.WriteLine(os);

            string path = InfoDll.AppDataProductLogPath;

            Console.WriteLine(CadInfo.Current);
        }

        public void TestCondole()
        {
            AC.Msg.InfoMessage($"{AC.AddOn.TitlePrefix} Message");

            AC.MsgCmd.ConsoleMessage($"{AC.AddOn.TitlePrefix} Console message");

            AC.MsgGUI.InfoMessage($"{AC.AddOn.TitlePrefix} Info message");


            //IApplicationInfo app = AC.Get<IApplicationInfo>();
            //IMessageService messageService = AC.Get<ICommandLineMessageService>();
            //messageService.ConsoleMessage($"{app.TitlePrefix}  Console message");

            //messageService = AC.Get<IWindowMessageService>();
            //messageService.InfoMessage("Info message");
        }
    }
}