using dRz.Abstractions.Services.Message;
using System;

namespace dRz.Common.NC
{
    /// <summary>
    /// Предоставляет служебным командам общий сервис сообщений, переданный аддоном.
    /// </summary>
    public static class CommonCommandContext
    {
        private static IMessageService? _messageService;

        /// <summary>
        /// Инициализирует контекст общим сервисом сообщений.
        /// </summary>
        /// <param name="messageService">Сервис сообщений, полученный из контейнера аддона.</param>
        /// <exception cref="ArgumentNullException">Сервис не задан.</exception>
        /// <exception cref="InvalidOperationException">Контекст уже инициализирован другим экземпляром сервиса.</exception>
        public static void Initialize(IMessageService messageService)
        {
            ArgumentNullException.ThrowIfNull(messageService);

            if (_messageService != null && !ReferenceEquals(_messageService, messageService))
            {
                throw new InvalidOperationException("Common.NC уже инициализирован другим сервисом сообщений.");
            }

            _messageService = messageService;
        }

        /// <summary>
        /// Возвращает сервис сообщений после инициализации контекста.
        /// </summary>
        /// <exception cref="InvalidOperationException">Контекст ещё не инициализирован.</exception>
        internal static IMessageService MessageService =>
            _messageService ?? throw new InvalidOperationException("Common.NC не инициализирован.");
    }
}
