using Teigha.Runtime;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.Loader.CadCommands.Message
{
    public class ConsoleMessageCmd
    {
        [CommandMethod($"console-message-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void ConsoleMessageCommand()
        {
            Msg.ConsoleMessage("test Console message");

            Msg.InfoMessage("test Console message");

            Msg.ErrorMessage("test Console message");
        }
    }
}