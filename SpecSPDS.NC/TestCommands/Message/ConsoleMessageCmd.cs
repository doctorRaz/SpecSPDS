#if DEBUG
using dRz.Abstractions.Services.Message;
using dRz.Src.Infrastructure;
using Teigha.Runtime;

namespace dRz.NC.TestCommands.Message
{
    public class ConsoleMessageCmd
    {
        private static IMessageService _msg = AddOnContext.Msg;


        [CommandMethod(CommandPrefix.Value + "_console-message", CommandFlags.Session)]
        public static void ConsoleMessageCommand()
        {
            _msg.InfoMessage("test Console message");

            _msg.InfoMessage("test Console message");

            _msg.WarningMessage("test Console message");
        }
    }
}
#endif