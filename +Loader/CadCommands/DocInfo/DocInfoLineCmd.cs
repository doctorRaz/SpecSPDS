using drz.Abstractions.Enums;
using drz.Abstractions.Factories;
using drz.Abstractions.Services;
using drz.Loader;
using Teigha.Runtime;

namespace Test.CadCommands.DocInfo
{
    public class DocInfoLineCmd
    {
        [CommandMethod($"-doc-info-{GeneratedCompile.CommandSuf}")]
        public static void DocInfoLineCommand()
        {
            IDocumentService documentService = EntryPoint.Container.GetInstance<IDocumentService>();
            IMessageServiceFactory messageFactory = EntryPoint.Container.GetInstance<IMessageServiceFactory>();
            IMessageService messageService = messageFactory.GetService(MessageServiceType.CommandLine);
            messageService.InfoMessage(documentService.FullPath);
        }
    }
}
