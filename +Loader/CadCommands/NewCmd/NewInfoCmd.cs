using Teigha.Runtime;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.Loader.CadCommands.NewCmd
{
    public class NewInfoCmd
    {
        [CommandMethod($"console-New-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void ConsoleNewCmd()
        {
            Msg.InfoMessage($"{CadInfo_NEW}");

            Msg.InfoMessage($"{SysInfo_NEW}");

            Msg.ErrorMessage($"{InfoDll_NEW}");

            Msg.ConsoleMessage($"{InfoDll_NEW.ToLongString()}");
        }

        [CommandMethod($"info-New-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void GuiNewCmd()
        {
            MsgGUI.InfoMessage($"{CadInfo_NEW}");

            MsgGUI.ConsoleMessage($"{SysInfo_NEW}");

            MsgGUI.InfoMessage($"{InfoDll_NEW}");

            MsgGUI.ConsoleMessage($"{InfoDll_NEW.ToLongString()}");
        }
    }
}