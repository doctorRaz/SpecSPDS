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

            Msg.ConsoleMessage($"{SysInfo_NEW}");

            Msg.ErrorMessage($"{InfoDll_NEW}");
           
        }

        [CommandMethod($"info-New-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void GuiNewCmd()
        {
            MsgGUI.InfoMessage($"{CadInfo_NEW}");

            MsgGUI.ConsoleMessage($"{SysInfo_NEW}");

            MsgGUI.ConsoleMessage($"{InfoDll_NEW.ToLongString()}");
        }

        [CommandMethod($"console-Long-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void ConsoleLongCmd()
        {
            
            Msg.ErrorMessage($"{InfoDll_NEW.ToLongString()}");

            Msg.ConsoleMessage($"{SysInfo_NEW.ToLongString()}");


        }
    }
}