#if DEBUG
using dRz.NC.Infrastructure;
using Rtm = Teigha.Runtime;
using Scm = System.ComponentModel;
namespace dRz.NC.TestCommands.Init
{
    public class InitCMD
    {

#if DEBUG && NC

        [Rtm.CommandMethod(CommandPrefix.Value + "_инит", Rtm.CommandFlags.Session)]
        [Scm.Description("ручной инит загрузчика для SPEC")]
        public static void test()
        {
            AddOnContext.Msg.InfoMessage($"инит SPEC");

            EntryPoint entryPoint = new EntryPoint();

            entryPoint.Initialize();
        }

        [Rtm.CommandMethod("-" + CommandPrefix.Value + "_console-message-test", Rtm.CommandFlags.Session)]
        public static void ConsoleMessageCommand()
        {
            AddOnContext.MsgCmd.ConsoleMessage("Console message");
        }

#endif

    }
}
#endif