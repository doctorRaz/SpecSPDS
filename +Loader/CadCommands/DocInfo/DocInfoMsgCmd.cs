using Teigha.Runtime;
using static dRz.Src.Infrastructure.AddOnContext;

namespace dRz.Loader.CadCommands.DocInfo
{
    public class DocInfoMsgCmd
    {
        [CommandMethod($"doc-info-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void DocInfoMessageCommand()
        {
            MsgGui.InfoMessage(DocService.FullPath);
        }
    }
}