using Teigha.Runtime;
using static dRz.Src.Infrastructure.AddOnContext;

namespace dRz.NC.TestCommands.Message
{
    public class GuiMessageCmd
    {
        [CommandMethod($"SPEC-info-message", CommandFlags.Session)]
        public static void InfoMessageCommand()
        {
            MsgGui.InfoMessage("Info message");

        }
        [CommandMethod($"SPEC-info-noti", CommandFlags.Session)]
        public static void InfoMessageNoti()
        {
            MsgMcN.InfoMessage("Info message");

        }
    }
}