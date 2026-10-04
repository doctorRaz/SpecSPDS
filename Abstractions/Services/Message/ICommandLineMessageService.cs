using System.Runtime.CompilerServices;

namespace dRz.Abstractions.Services.Message
{
    /// <summary>Сервис сообщений ком строка 
    /// добавлен метод </summary>
    /// <seealso cref="IMessageService" ConsoleMessage />
    public interface ICommandLineMessageService : IMessageService
    {

        /// <summary> Сообщение в консоль CAD </summary>
        /// <param name="message">Выводимое сообщение</param>
        /// <param name="caller">Вызывающий метод. null - вычисляется автоматически. В случае, если ком.строка недоступна,<br/>
        /// рекомендуется реализовывать вызов <see cref="InfoMessage"/> с теми же параметрам </param>
        /// <example>Пример использования в C#
        /// <code language="cs">
        /// <![CDATA[
        /// IMessageService msgService = new MyMessageService();
        /// msgService.ConsoleMessage("Консольное сообщение");
        /// ]]>
        /// </code></example>
        /// <remarks>В зависимости от основной CAD-системы может понадобиться в реализации обрамлять сообщение символами перевода строки (\n)</remarks>
        void ConsoleMessage(string message, [CallerMemberName] string caller = null);
    }
}