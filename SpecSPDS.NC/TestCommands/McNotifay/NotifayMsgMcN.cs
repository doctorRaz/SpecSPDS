#if DEBUG
using Teigha.Runtime;
using dRz.Src.Infrastructure;
using dRz.Abstractions.Services.Message;

namespace dRz.NC.TestCommands.McNotifay
{
    public class NotifayMsgMcN
    {

        private static IMcNotificatorMessageService _msgMcN = AddOnContext.MsgN;

        static System.Exception ex = new System.Exception("Properties is null");

        [CommandMethod(CommandPrefix.Value + "SPEC-noti1", CommandFlags.Session)]
        public static void noti1()
        {
            _msgMcN.InfoMessage("Без иконки");
        }

        [CommandMethod(CommandPrefix.Value + "SPEC-noti2", CommandFlags.Session)]
        public static void noti2()
        {

            _msgMcN.ErrorMessage("С иконкой err", ex);
        }

        [CommandMethod(CommandPrefix.Value + "SPEC-noti20", CommandFlags.Session)]
        public static void noti20()
        {

            _msgMcN.ErrorMessage("С иконкой err");
        }

        [CommandMethod(CommandPrefix.Value + "SPEC-noti3", CommandFlags.Session)]
        public static void noti3()
        {
            _msgMcN.ErrorMessage(ex);
        }

        [CommandMethod(CommandPrefix.Value + "SPEC-noti4", CommandFlags.Session)]
        public static void InfoMessageNoti()
        {
            _msgMcN.WarningMessage($"{AddOnContext.SysInfo.ToLongString()}");

        }
    }
}
#endif