using Abstractions.Enums;
using Abstractions.Factories;
using Abstractions.Services;
using Teigha.Runtime;

namespace NCad.CadCommands.DocInfo
{
    public class DocInfoLineCmd
    {
        [CommandMethod("-doc-info")]
        public static void DocInfoLineCommand()
        {
            IDocumentService documentService = CadPlugin.Container.GetInstance<IDocumentService>();
            IMessageServiceFactory messageFactory = CadPlugin.Container.GetInstance<IMessageServiceFactory>();
            IMessageService messageService = messageFactory.GetService(MessageServiceType.CommandLine);
            messageService.InfoMessage(documentService.FullPath);
        }
    }
}
