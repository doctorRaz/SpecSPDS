using Abstractions.Services;
using Teigha.Runtime;

namespace Test.CadCommands
{
    public class InfoMessageCmd
    {
        [CommandMethod("info-message")]
        public static void InfoMessageCommand()
        {
            IMessageService messageService = CadPlugin.Container.GetInstance<IMessageService>();
            messageService.InfoMessage("Info message");
        }
    }
}
