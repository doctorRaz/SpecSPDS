#if DEBUG
using dRz.Src.Infrastructure;
using Teigha.Runtime;

namespace dRz.NC.TestCommands.Message
{
    public class GuiMessageCmd
    {

        [CommandMethod(CommandPrefix.Value + "SPEC-info-message", CommandFlags.Session)]
        public static void InfoMessageCommand()
        {
            AddOnContext.MsgW.InfoMessage("Info message");

        }
    }
}
#endif