#if DEBUG
using Teigha.Runtime;
using static dRz.Src.Infrastructure.AddOnContext;

namespace dRz.NC.TestCommands.DocInfo
{
    public class DocInfoLineCmd
    {
        [CommandMethod($"-SPEC-doc-info", CommandFlags.Session)]
        public static void DocInfoLineCommand()
        {
            Msg.InfoMessage(DocService.FullPath);
        }
    }
}
#endif