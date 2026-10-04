using Teigha.Runtime;
using static dRz.Src.Infrastructure.AddOnContext;

namespace dRz.Loader.CadCommands.Message
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