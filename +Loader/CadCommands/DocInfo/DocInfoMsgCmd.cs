using Teigha.Runtime;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.Loader.CadCommands.DocInfo
{
    public class DocInfoMsgCmd
    {
        [CommandMethod($"doc-info-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void DocInfoMessageCommand()
        {
            MsgGUI.InfoMessage(DocService.FullPath);
        }
    }
}