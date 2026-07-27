using Teigha.Runtime;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.Loader.CadCommands.Message
{
    public class GuiMessageCmd
    {
        [CommandMethod($"info-message-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void InfoMessageCommand()
        {
            MsgGui.InfoMessage("Info message");
          
        }
    }
}