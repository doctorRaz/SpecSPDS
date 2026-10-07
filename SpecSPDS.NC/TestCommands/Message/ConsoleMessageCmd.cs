using Teigha.Runtime;
using static dRz.Src.Infrastructure.AddOnContext;

namespace dRz.NC.TestCommands.Message
{
    public class ConsoleMessageCmd
    {
        [CommandMethod($"SPEC-console-message", CommandFlags.Session)]
        public static void ConsoleMessageCommand()
        {
            Msg.InfoMessage("test Console message");

            Msg.InfoMessage("test Console message");

            Msg.WarningMessage("test Console message");
        }
    }
}