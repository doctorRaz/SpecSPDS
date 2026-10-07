#if DEBUG
using static dRz.Src.Infrastructure.AddOnContext;
using Rtm = Teigha.Runtime;
using Scm = System.ComponentModel;
namespace dRz.NC.TestCommands.Init
{
    public class InitCMD
    {

#if DEBUG && NC

        [Rtm.CommandMethod($"SPEC-инит", Rtm.CommandFlags.Session)]
        [Scm.Description($"ручной инит загрузчика для SPEC")]
        public static void test()
        {
            Msg.InfoMessage($"инит SPEC");
            EntryPoint entryPoint = new EntryPoint();
            entryPoint.Initialize();
        }

        [Rtm.CommandMethod($"-SPEC-console-message-test", Rtm.CommandFlags.Session)]
        public static void ConsoleMessageCommand()
        {
            MsgCmd.ConsoleMessage("Console message");
        }

#endif

    }
}
#endif