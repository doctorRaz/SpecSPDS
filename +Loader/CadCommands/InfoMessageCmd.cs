using Teigha.Runtime;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.Loader.CadCommands
{
    public class InfoMessageCmd
    {
        [CommandMethod($"info-message-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void InfoMessageCommand()
        {
            MsgGUI.InfoMessage("Info message");
        }
    }
}