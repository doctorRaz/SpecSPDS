using drz.Abstractions.Enums;
using drz.Abstractions.Factories;
using drz.Abstractions.Services;
using drz.Loader;
using Teigha.Runtime;

namespace Test.CadCommands.DocInfo
{
    public class DocInfoMsgCmd
    {
        [CommandMethod($"doc-info-{GeneratedCompile.CommandSuf}")]
        public static void DocInfoMessageCommand()
        {
            IDocumentService documentService = EntryPoint.Container.GetInstance<IDocumentService>();
            IMessageServiceFactory messageFactory = EntryPoint.Container.GetInstance<IMessageServiceFactory>();
            IMessageService messageService = messageFactory.GetService(MessageServiceType.Window);
            messageService.InfoMessage(documentService.FullPath);
        }
    }
}