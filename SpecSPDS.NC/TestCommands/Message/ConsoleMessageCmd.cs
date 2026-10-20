#if DEBUG
using Teigha.Runtime;
using dRz.Src.Infrastructure;
using dRz.Abstractions.Services.Message;

namespace dRz.NC.TestCommands.Message
{
    public class ConsoleMessageCmd
    {
        private static IMessageService _msg = AddOnContext.Msg;


        [CommandMethod(CommandPrefix.Value + "SPEC-console-message", CommandFlags.Session)]
        public static void ConsoleMessageCommand()
        {
            _msg.InfoMessage("test Console message");

            _msg.InfoMessage("test Console message");

            _msg.WarningMessage("test Console message");
        }
    }
}
#endif