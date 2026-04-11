using drz.Abstractions.Services;
using Teigha.Runtime;

namespace drz.Loader.CadCommands
{
    public class InfoMessageCmd
    {
        [CommandMethod($"info-message-{GeneratedCompile.CommandSuf}")]
        public static void InfoMessageCommand()
        {
            IMessageService messageService = EntryPoint.Container.GetInstance<IMessageService>();
            messageService.InfoMessage("Info message");
        }
    }
}