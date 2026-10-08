using dRz.Abstractions.Services;
using dRz.Abstractions.Services.Message;
using System;
using System.Runtime.CompilerServices;

namespace dRz.Infrastructure.Services.Message
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
    public sealed class MessageService : IMessageService, IMessagePromptService
    {
        #region Private Fields

        private readonly ICommandLineMessageService _commandLine;
        private readonly IDocumentService _documentService;
        private readonly IWindowMessageService _window;

        #endregion Private Fields

        #region Public Constructors

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

        #endregion Public Constructors

        #region Private Properties

        /// <summary>
        /// Возвращает текущий способ вывода сообщения.
        /// </summary>
        private IMessageService Current =>
            _documentService.IsActive
                ? _commandLine
                : _window;

        #endregion Private Properties

        #region Public Methods

        public void ErrorMessage(Exception ex, [CallerMemberName] string? caller = null)
        {
            Current.ErrorMessage(ex, caller);
        }

        public void ErrorMessage(string message, Exception? ex = null, [CallerMemberName] string? caller = null)
        {
            Current.ErrorMessage(message, ex, caller);
        }

        public void InfoMessage(string message, [CallerMemberName] string? caller = null)
        {
            Current.InfoMessage(message, caller);
        }

        public void WarningMessage(string message, [CallerMemberName] string? caller = null)
        {
            Current.WarningMessage(message, caller);
        }

        public MessageResult AskYesNo(string message, string title, [CallerMemberName] string? caller = null)
        {
            return _window.AskYesNo(message, title, caller);
        }

        #endregion Public Methods
    }
}