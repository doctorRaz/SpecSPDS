using Teigha.Runtime;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.Loader.CadCommands.NewCmd
{
    public class NewInfoCmd
    {
        [CommandMethod($"console-New-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void ConsoleNewCmd()
        {
            Msg.InfoMessage($"{CadInfo}");

            Msg.ConsoleMessage($"{SysInfo}");

            Msg.ErrorMessage($"{AddonInfo}");
           
        }

        [CommandMethod($"info-New-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void GuiNewCmd()
        {
            MsgGUI.InfoMessage($"{CadInfo}");

            MsgGUI.ConsoleMessage($"{SysInfo}");

            MsgGUI.ConsoleMessage($"{AddonInfo.ToLongString()}");
        }

        [CommandMethod($"console-Long-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void ConsoleLongCmd()
        {
            
            Msg.ErrorMessage($"{AddonInfo.ToLongString()}");

            Msg.ConsoleMessage($"{SysInfo.ToLongString()}");


        }
    }
}