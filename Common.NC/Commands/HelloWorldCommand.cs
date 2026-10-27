using dRz.Abstractions.Services.Message;
using Teigha.Runtime;

namespace dRz.Common.NC.Commands
{
    /// <summary>
    /// Проверочная служебная команда Common.NC.
    /// </summary>
    public sealed class HelloWorldCommand
    {
        /// <summary>
        /// Выводит приветственное сообщение через общий сервис сообщений.
        /// </summary>
        [CommandMethod("drz_SpecSPDS_HelloWorld", CommandFlags.Session)]
        public static void HelloWorld()
        {
            CommonCommandContext.Services.Get<IMessageService>()
                .InfoMessage("Hello World from Common.NC");
        }
    }
}
