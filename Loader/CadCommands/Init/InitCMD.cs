using dRz.Loader;
using static dRz.Src.Infrastructure.AddOnContext;
using Rtm = Teigha.Runtime;
using Scm = System.ComponentModel;
namespace dRz.Loader.CadCommands.Init
{
    public class InitCMD
    {

#if DEBUG && NC

        [Rtm.CommandMethod($"инит_{GeneratedCompile.CommandSuf}", Rtm.CommandFlags.Session)]
        [Scm.Description($"ручной инит загрузчика для {GeneratedCompile.CommandSuf}")]
        public static void test()
        {
            Msg.InfoMessage($"инит {GeneratedCompile.CommandSuf}");
            EntryPoint entryPoint = new EntryPoint();
            entryPoint.Initialize();
        }

        [Rtm.CommandMethod($"-console-message-test-{GeneratedCompile.CommandSuf}", Rtm.CommandFlags.Session)]
        public static void ConsoleMessageCommand()
        {
            MsgCmd.ConsoleMessage("Console message");
        }

#endif

    }
}
