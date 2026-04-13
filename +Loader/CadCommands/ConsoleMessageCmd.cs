using Teigha.Runtime;
using static drz.Loader.Infrastructure.AddOnContext;

namespace drz.Loader.CadCommands
{
    public class ConsoleMessageCmd
    {
        [CommandMethod($"console-message-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void ConsoleMessageCommand()
        {

            MsgCmd.ConsoleMessage("test Console message");
        }
    }
}