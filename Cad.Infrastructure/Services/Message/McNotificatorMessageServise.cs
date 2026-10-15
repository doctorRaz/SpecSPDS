using drz.MulticadInterop.McNotificator;
using dRz.Abstractions.Services.Message;
using System;
using System.Runtime.CompilerServices;

namespace dRz.NC.Infrastructure.Services.Message
{
    /// <summary>Обёртка над McNotificator NanoCAD для вывода уведомлений.</summary>
    public class McNotificatorMessageServise : IMcNotificatorMessageService
    {
        public void WarningMessage(string message, [CallerMemberName] string? caller = null, string? title = null) =>
            WriteMessage(NotificationType.neWarning, message, caller, title);

        public void ErrorMessage(Exception ex, [CallerMemberName] string? caller = null, string? title = null) =>
            WriteMessage(NotificationType.neError, $"{ex.Message}\n{ex.StackTrace}", caller, title);

        public void ErrorMessage(string message, Exception? ex = null, [CallerMemberName] string? caller = null, string? title = null) =>
            WriteMessage(NotificationType.neError,
                ex == null ? message : $"{message}\n{ex.Message}\n{ex.StackTrace}", caller, title);

        public void InfoMessage(string message, [CallerMemberName] string? caller = null, string? title = null) =>
            WriteMessage(NotificationType.neUsual, message, caller, title);

        public static void WriteMessage(NotificationType notificationType, string message, string? caller, string? title = null) =>
            MulticadNotificator.CreateMessage(Formatted(message, caller, title), notificationType);

        public static void WriteMessage(string message, string? caller) =>
            MulticadNotificator.CreateMessage(Formatted(message, caller, null), NotificationType.neCure);

        private static string Formatted(string message, string? caller, string? title)
        {
            string source = string.IsNullOrWhiteSpace(title) ? string.Empty : $"[{title}] ";
            return source + (string.IsNullOrWhiteSpace(caller) ? string.Empty : $"{caller} >> ") + message;
        }
    }
}