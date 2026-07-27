using System;
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

            Msg.InfoMessage($"{SysInfo}");

            Msg.WarningMessage($"{AddOnInfo}");

        }

        [CommandMethod($"info-New-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void GuiNewCmd()
        {
            MsgGui.InfoMessage($"{CadInfo}");

            MsgGui.WarningMessage($"{SysInfo}");

            System.Exception ex = new System.Exception("test err");
            MsgGui.ErrorMessage($"{AddOnInfo.ToLongString()}", ex);
        }

        [CommandMethod($"console-Long-{GeneratedCompile.CommandSuf}", CommandFlags.Session)]
        public static void ConsoleLongCmd()
        {

            Msg.WarningMessage($"{AddOnInfo.ToLongString()}");

            Msg.InfoMessage($"{SysInfo.ToLongString()}");


        }
    }
}