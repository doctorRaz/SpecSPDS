#if DEBUG
using Teigha.Runtime;
using static dRz.Src.Infrastructure.AddOnContext;

namespace dRz.NC.TestCommands.McNotifay
{
    public class NotifayMsgMcN
    {

        static System.Exception ex = new System.Exception("Properties is null");

        [CommandMethod($"SPEC-noti1", CommandFlags.Session)]
        public static void noti1()
        {
            MsgMcN.InfoMessage("Без иконки");
        }

        [CommandMethod($"SPEC-noti2", CommandFlags.Session)]
        public static void noti2()
        {

            MsgMcN.ErrorMessage("С иконкой err", ex);
        }

        [CommandMethod($"SPEC-noti20", CommandFlags.Session)]
        public static void noti20()
        {

            MsgMcN.ErrorMessage("С иконкой err");
        }

        [CommandMethod($"SPEC-noti3", CommandFlags.Session)]
        public static void noti3()
        {
            MsgMcN.ErrorMessage(ex);
        }

        [CommandMethod($"SPEC-noti4", CommandFlags.Session)]
        public static void InfoMessageNoti()
        {
            MsgMcN.WarningMessage($"{SysInfo.ToLongString()}");

        }
    }
}
#endif