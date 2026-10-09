using System.Runtime.CompilerServices;

namespace dRz.Abstractions.Services.Message
{
    /// <summary>Сервис интерактивных сообщений.</summary>
    public interface IMessagePromptService
    {
        /// <summary>Запрос подтверждения действия у пользователя.</summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="title">Заголовок окна.</param>
        /// <param name="caller">Вызывающий метод. null - вычисляется автоматически.</param>
        MessageResult AskYesNo(string message, string title, [CallerMemberName] string? caller = null);
    }
}