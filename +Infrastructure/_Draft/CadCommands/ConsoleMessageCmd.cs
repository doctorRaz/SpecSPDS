using Abstractions.Services;
using Teigha.Runtime;

namespace Test.CadCommands
{
    public class ConsoleMessageCmd
    {
        [CommandMethod("console-message")]
        public static void ConsoleMessageCommand()
        {
            IMessageService messageService = CadPlugin.Container.GetInstance<IMessageService>();
            messageService.ConsoleMessage("Console message");
        }
    }
}
