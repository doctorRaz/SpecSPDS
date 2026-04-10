using Abstractions.Services;
using drz.Cad.Diagnostics.Cad;
using drz.Cad.Diagnostics.Os;
using System;
using static drz.Loader.Infrastructure.AddonContext;
using SimpleInjector;
using Abstractions.Enums;
using Abstractions.Factories;

namespace drz.SpecSpds.Test.Tests
{
    internal class Class1
    {

        public void msg()
        {
            IMessageService messageService = Simple.Container.GetInstance<IMessageService>();
            messageService.ConsoleMessage("Console message");


            messageService = Simple.Container.GetInstance<IMessageService>();
            messageService.InfoMessage("Info message");

            //IDocumentService documentService = Simple.Container.GetInstance<IDocumentService>();
            IMessageServiceFactory messageFactory = Simple.Container.GetInstance<IMessageServiceFactory>();
            messageService = messageFactory.GetService(MessageServiceType.CommandLine);
            messageService.InfoMessage("documentService.FullPath");



            //IDocumentService documentService2 = Simple.Container.GetInstance<IDocumentService>();
            messageFactory = Simple.Container.GetInstance<IMessageServiceFactory>();
            messageService = messageFactory.GetService(MessageServiceType.Window);
            messageService.InfoMessage("documentService.FullPath");


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
