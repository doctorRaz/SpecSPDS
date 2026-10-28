using dRz.Common.NC.Infrastructure;
using Teigha.Runtime;

namespace dRz.Common.NC.Commands
{
    /// <summary>
    /// Проверочная служебная команда Common.Common.NC.
    /// </summary>
    public sealed class HelloWorldCommand
    {
        /// <summary>
        /// Выводит приветственное сообщение через общий сервис сообщений.
        /// </summary>
        [CommandMethod("drz_SpecSPDS_HelloWorld", CommandFlags.Session)]
        public static void HelloWorld()
        {
            AddOnContext.Msg.InfoMessage("Hello World from Common.Common.NC");
        }
    }
}
