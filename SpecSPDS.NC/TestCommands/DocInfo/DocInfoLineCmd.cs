#if DEBUG
using dRz.Src.Infrastructure;
using Teigha.Runtime;
//using static dRz.Src.Infrastructure.AddOnContext;

namespace dRz.NC.TestCommands.DocInfo
{
    public class DocInfoLineCmd
    {
        [CommandMethod("-" + CommandPrefix.Value + "_doc-info", CommandFlags.Session)]
        public static void DocInfoLineCommand()
        {
            AddOnContext.Msg.InfoMessage(AddOnContext.DocService.FullPath);
        }
    }
}
#endif