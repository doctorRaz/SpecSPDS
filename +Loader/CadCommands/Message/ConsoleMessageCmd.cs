using Teigha.Runtime;
using static dRz.Src.Infrastructure.AddOnContext;

namespace dRz.Loader.CadCommands.Message
{
    public class ConsoleMessageCmd
    {
        [CommandMethod($"console-message-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void ConsoleMessageCommand()
        {
            Msg.InfoMessage("test Console message");

            Msg.InfoMessage("test Console message");

            Msg.WarningMessage("test Console message");
        }
    }
}