#if DEBUG
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
    }
}
#endif