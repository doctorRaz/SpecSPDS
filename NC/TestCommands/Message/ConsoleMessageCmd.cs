#if DEBUG
using dRz.Abstractions.Services.Message;
using dRz.NC.Infrastructure;
using Teigha.Runtime;

namespace dRz.NC.TestCommands.Message
{
    public class ConsoleMessageCmd
    {
        private IMessageService _msg = AddOnContext.Msg;


        [CommandMethod(CommandPrefix.Value + "_console-message", CommandFlags.Session)]
        public void ConsoleMessageCommand()
        {
            _msg.InfoMessage("InfoMessage");

            _msg.WarningMessage("WarningMessage");

            _msg.ErrorMessage("ErrorMessage");
        }
    }
}
#endif