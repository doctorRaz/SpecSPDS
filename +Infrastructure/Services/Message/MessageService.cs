using drz.Abstractions.Services;
using drz.Abstractions.Services.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace drz.Infrastructure.Services.Message
{
    /// <summary>
    /// Универсальный сервис сообщений.
    ///
    /// В зависимости от состояния nanoCAD выбирает способ вывода:
    /// - при наличии активного документа —  командная строка
    /// - без документа — оконный вывод;.
    ///
    /// Пользователь класса не должен знать, куда будет выведено сообщение.
    /// </summary>
    public sealed class MessageService : IMessageService
    {
        private readonly ICommandLineMessageService _commandLine;
        private readonly IDocumentService _documentService;
        private readonly IWindowMessageService _window;
        /// <summary>
        /// Создает сервис маршрутизации сообщений.
        /// </summary>
        /// <param name="commandLine">
        /// Сервис вывода в командную строку.
        /// </param>
        /// <param name="window">
        /// Сервис оконного вывода.
        /// </param>
        /// <param name="documentService">
        /// Сервис информации о текущем документе.
        /// </param>
        public MessageService(
            ICommandLineMessageService commandLine,
            IWindowMessageService window,
            IDocumentService documentService)
        {
            _commandLine = commandLine
                ?? throw new ArgumentNullException(nameof(commandLine));

            _window = window
                ?? throw new ArgumentNullException(nameof(window));

            _documentService = documentService
                ?? throw new ArgumentNullException(nameof(documentService));
        }

        /// <summary>
        /// Возвращает текущий способ вывода сообщения.
        /// </summary>
        private IMessageService Current =>
            _documentService.IsActive
                ? _commandLine
                : _window;

        //public void ConsoleMessage(string message, [CallerMemberName] string? caller = null)
        //{
        //    Current.ConsoleMessage(message, caller);
        //}

        public void ErrorMessage(Exception ex, [CallerMemberName] string? caller = null)
        {
            Current.ErrorMessage(ex, caller);
        }

        public void ErrorMessage(string message,Exception ex =null, [CallerMemberName] string? caller = null)
        {
            Current.ErrorMessage(  message, ex,caller);
        }

        public void InfoMessage(string message, [CallerMemberName] string? caller = null)
        {
            Current.InfoMessage(message, caller);
        }

        public void WarningMessage(string message, [CallerMemberName] string? caller = null)
        {
            Current.WarningMessage(message, caller);
        }
    }
}