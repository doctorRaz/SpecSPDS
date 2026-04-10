using Abstractions.Services;
using Teigha.Runtime;

namespace NCad.CadCommands
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
