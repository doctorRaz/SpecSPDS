using System;
using Teigha.Runtime;
using static dRz.Src.Infrastructure.AddOnContext;


namespace dRz.NC.TestCommands.NewCmd
{
    public class NewInfoCmd
    {
        [CommandMethod($"SPEC-console-New", CommandFlags.Session)]
        public static void ConsoleNewCmd()
        {
            Msg.InfoMessage($"{CadInfo}");

            Msg.InfoMessage($"{SysInfo}");

            Msg.WarningMessage($"{AddOnInfo}");

        }

        [CommandMethod($"SPEC-info-New", CommandFlags.Session)]
        public static void GuiNewCmd()
        {
            MsgGui.InfoMessage($"{CadInfo}");

            MsgGui.WarningMessage($"{SysInfo}");

            System.Exception ex = new System.Exception("test err");
            MsgGui.ErrorMessage($"{AddOnInfo.ToLongString()}", ex);
        }

        [CommandMethod($"SPEC-console-Long", CommandFlags.Session)]
        public static void ConsoleLongCmd()
        {

            Msg.WarningMessage($"{AddOnInfo.ToLongString()}");

            Msg.InfoMessage($"{SysInfo.ToLongString()}");


        }
    }
}