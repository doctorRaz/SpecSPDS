using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using drz.Cad.Diagnostics.Cad;
using drz.Cad.Diagnostics.Os;
using System;
using static drz.Loader.Infrastructure.AddonContext;
using static drz.SpecSpds.Test.Start;

namespace drz.SpecSpds.Test.Tests
{
    internal class TestContainer
    {
        public static void TestCondole()
        {
            IApplicationInfo app = ContainerIn.GetInstance<IApplicationInfo>();
            IMessageService messageService = ContainerIn.GetInstance<ICommandLineMessageService>();
            messageService.ConsoleMessage($"{app.TitlePrefix}  Console message");

            messageService = ContainerIn.GetInstance<IWindowMessageService>();
            messageService.InfoMessage("Info message");
        }

        public void test()
        {
            InfoOs os = InfoOs.Current;

            Console.WriteLine(os);

            string path = InfoDll.AppDataProductLogPath;

            Console.WriteLine(InfoCad.Current);
        }
    }
}