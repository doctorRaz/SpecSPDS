using Abstractions.Services;
using Teigha.Runtime;

namespace NCad.CadCommands
{
    public class ConsoleMessageCmd
    {
        [CommandMethod("console-message")]
        public static void ConsoleMessageCommand()
        {
            IMessageService messageService = CadPlugin.Container.GetInstance<IMessageService>();
            messageService.Console("Console message");
        }
    }
}
