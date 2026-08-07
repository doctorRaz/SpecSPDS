using Teigha.Runtime;
using static dRz.Src.Infrastructure.AddOnContext;

namespace dRz.Loader.CadCommands.DocInfo
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