#if DEBUG
using dRz.Src.Infrastructure;
using Teigha.Runtime;
//using static dRz.Src.Infrastructure.AddOnContext;

namespace dRz.NC.TestCommands.DocInfo
{
    public class DocInfoMsgCmd
    {
        [CommandMethod(CommandPrefix.Value + "_doc-info", CommandFlags.Session)]
        public static void DocInfoMessageCommand()
        {
            AddOnContext.MsgW.InfoMessage(AddOnContext.DocService.FullPath);
        }
    }
}
#endif
