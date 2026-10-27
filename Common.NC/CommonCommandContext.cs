using dRz.Abstractions.Services;
using System;

namespace dRz.Common.NC
{
    /// <summary>
    /// Предоставляет служебным командам доступ к сервисам, переданным аддоном.
    /// </summary>
    public static class CommonCommandContext
    {
        private static IAddOnServices? _services;

        /// <summary>
        /// Инициализирует контекст сервисами аддона.
        /// </summary>
        /// <param name="services">Сервисы, зарегистрированные в контейнере аддона.</param>
        /// <exception cref="ArgumentNullException">Сервисы не заданы.</exception>
        /// <exception cref="InvalidOperationException">Контекст уже инициализирован другим экземпляром сервисов.</exception>
        public static void Initialize(IAddOnServices services)
        {
            ArgumentNullException.ThrowIfNull(services);

            if (_services != null && !ReferenceEquals(_services, services))
            {
                throw new InvalidOperationException("Common.NC уже инициализирован другими сервисами.");
            }

            _services = services;
        }

        /// <summary>
        /// Возвращает сервисы аддона после инициализации контекста.
        /// </summary>
        /// <exception cref="InvalidOperationException">Контекст ещё не инициализирован.</exception>
        internal static IAddOnServices Services =>
            _services ?? throw new InvalidOperationException("Common.NC не инициализирован.");
    }
}
