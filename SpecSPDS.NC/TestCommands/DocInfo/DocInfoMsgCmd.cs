using Teigha.Runtime;
using static dRz.Src.Infrastructure.AddOnContext;

namespace dRz.NC.TestCommands.DocInfo
{
    public class DocInfoMsgCmd
    {
        [CommandMethod($"SPEC-doc-info", CommandFlags.Session)]
        public static void DocInfoMessageCommand()
        {
            MsgGui.InfoMessage(DocService.FullPath);
        }
    }
}