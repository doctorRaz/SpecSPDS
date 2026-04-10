using Abstractions.Enums;
using Abstractions.Factories;
using Abstractions.Services;
using drz.Loader;
using Teigha.Runtime;

namespace Test.CadCommands.DocInfo
{
    public class DocInfoMsgCmd
    {
        [CommandMethod($"doc-info-{GeneratedCompile.CommandSuf}")]
        public static void DocInfoMessageCommand()
        {
            IDocumentService documentService = CadPlugin.Container.GetInstance<IDocumentService>();
            IMessageServiceFactory messageFactory = CadPlugin.Container.GetInstance<IMessageServiceFactory>();
            IMessageService messageService = messageFactory.GetService(MessageServiceType.Window);
            messageService.InfoMessage(documentService.FullPath);
        }
    }
}
