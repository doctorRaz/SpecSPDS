using drz.SpecSPDS.Abstractions.Services;
using Teigha.Runtime;

namespace drz.SpecSPDS.Infrastructure.CadCommands.DocInfo
{
    public class DocInfoMsgCmd
    {
        [CommandMethod("doc-info")]
        public static void DocInfoMessageCommand()
        {
            IDocumentService documentService = CadPlugin.Container.GetInstance<IDocumentService>();
            IMessageService messageService = CadPlugin.Container.GetInstance<IMessageService>();
            messageService.InfoMessage(documentService.FullPath);
        }
    }
}
