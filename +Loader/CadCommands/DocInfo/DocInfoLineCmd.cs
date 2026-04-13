using Teigha.Runtime;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.Loader.CadCommands.DocInfo
{
    public class DocInfoLineCmd
    {
        [CommandMethod($"-doc-info-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void DocInfoLineCommand()
        {
            Msg.InfoMessage(DocService.FullPath);
        }
    }
}