using Abstractions.Enums;
using Abstractions.Services;

namespace Abstractions.Factories
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
