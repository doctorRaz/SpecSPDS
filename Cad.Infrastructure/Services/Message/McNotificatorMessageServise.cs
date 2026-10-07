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
        //public void ConsoleMessage(string message, [CallerMemberName] string caller = null)
        //{
        //    throw new NotImplementedException();
        //}

        public void WarningMessage(string message, [CallerMemberName] string? caller = null)
        {
            throw new NotImplementedException();
        }

        public void ErrorMessage(Exception ex, [CallerMemberName] string? caller = null)
        {
            throw new NotImplementedException();
        }

        public void ErrorMessage(string message, Exception? ex = null, [CallerMemberName] string? caller = null)
        {
            throw new NotImplementedException();
        }

        public void InfoMessage(string message, [CallerMemberName] string? caller = null)
        {
            WriteMessage(message);
        }

        /// <summary>
        /// Выводит сообщение в командную строку NanoCad.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается если McNotificator.CreateMessage не найден в загруженных сборках.
        /// </exception>
        public static void WriteMessage(string message)
        {
            MulticadNotificator.WriteMessage(message);
        }
    }
}