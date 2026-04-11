using drz.Abstractions.Services;
using drz.Loader;
using Teigha.Runtime;

namespace Test.CadCommands
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