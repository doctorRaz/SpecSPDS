using drz.SpecSPDS.Abstractions.Services;
using Teigha.Runtime;

namespace drz.SpecSPDS.Infrastructure.CadCommands.DocInfo
{
    public class DocInfoLineCmd
    {
        [CommandMethod("-doc-info")]
        public static void DocInfoLineCommand()
        {
            IDocumentService documentService = CadPlugin.Container.GetInstance<IDocumentService>();
            IMessageService messageService = CadPlugin.Container.GetInstance<IMessageService>();
            messageService.Console(documentService.FullPath);
        }
    }
}
