using drz.MulticadInterop;
using Teigha.Runtime;
using static dRz.Src.Infrastructure.AddOnContext;

namespace dRz.NC.TestCommands.Message
{
    public class NotifayMessage
    {
        [CommandMethod($"SPEC-noti1", CommandFlags.Session)]
        public static void noti1()
        {
            MulticadNotificator.CreateMessage("Без иконки");
        }

        [CommandMethod($"SPEC-noti2", CommandFlags.Session)]
        public static void noti2()
        {
            MulticadNotificator.CreateMessage("С иконкой err", NotificationType.neError);
        }
        [CommandMethod($"SPEC-noti3", CommandFlags.Session)]
        public static void noti3()
        {
            MulticadNotificator.CreateMessage("С иконкой hint UIntPtr.Zero", NotificationType.neHint, UIntPtr.Zero);
        }
    }
}