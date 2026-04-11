using drz.Abstractions.Services;
using drz.Loader;
using Teigha.Runtime;

namespace Test.CadCommands
{
    public class ConsoleMessageCmd
    {
        [CommandMethod($"console-message-{GeneratedCompile.CommandSuf}")]
         
        public static void ConsoleMessageCommand()
        {
            IMessageService messageService = EntryPoint.Container.GetInstance<IMessageService>();
            messageService.ConsoleMessage("test Console message");
        }
    }
}
