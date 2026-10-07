using drz.MulticadInterop.McNotificator;
using dRz.Abstractions.Services.Message;
using System.Runtime.CompilerServices;

namespace dRz.NC.Infrastructure.Services.Message
{
    /// <summary>
    /// Обёртка над McNotificator NanoCad для вывода сообщений в командную строку.
    /// Работает без подключения сборки Multicad через Reflection.
    /// </summary>
    public class McNotificatorMessageServise : IMcNotificatorMessageService
    {
        public void WarningMessage(string message, [CallerMemberName] string? caller = null)
        {
            WriteMessage(NotificationType.neWarning, message, caller);
        }

        public void ErrorMessage(Exception ex, [CallerMemberName] string? caller = null)
        {
            WriteMessage(NotificationType.neError, $"{ex.Message}\n{ex.StackTrace}", caller);
        }

        public void ErrorMessage(string message, Exception? ex = null, [CallerMemberName] string? caller = null)
        {
            WriteMessage(NotificationType.neError,
                            ex == null
                            ? message
                            : $"{message}\n{ex.Message}\n{ex.StackTrace}", caller);
        }

        public void InfoMessage(string message, [CallerMemberName] string? caller = null)
        {
            WriteMessage(NotificationType.neUsual, message, caller);
        }

        /// <summary>
        /// Выводит сообщение Multicad.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается если McNotificator.CreateMessage не найден в загруженных сборках.
        /// </exception>
        public static void WriteMessage(NotificationType notificationType, string message, string? caller)
        {
            MulticadNotificator.CreateMessage(Formatted(message, caller), notificationType);
        }

        /// <summary>Выводит сообщение Multicad. <br/>
        /// fallback для консоли
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="caller">The caller.</param>
        public static void WriteMessage(string message, string? caller)
        {
            MulticadNotificator.CreateMessage(Formatted(message, caller), NotificationType.neCure);
        }

        private static string Formatted(string message, string? caller)
        {
            string format = (string.IsNullOrWhiteSpace(caller) ? "" : $"{caller} >> ") + message;

            return format;
        }
    }
}