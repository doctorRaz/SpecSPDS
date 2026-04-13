using drz.Abstractions.Enums;
using drz.Abstractions.Services;

namespace drz.Abstractions.Factories
{
    public interface IMessageServiceFactory
    {
        /// <summary>
        /// Получить реализации <see cref="IMessageService"/> в зависимости от запрашиваемого типа
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        IMessageService GetService(MessageServiceType messageType);
    }
}