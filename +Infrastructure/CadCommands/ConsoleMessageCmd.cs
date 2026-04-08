using drz.SpecSPDS.Abstractions.Services;
using Teigha.Runtime;

namespace drz.SpecSPDS.Infrastructure.CadCommands
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
