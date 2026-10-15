using dRz.Abstractions.Services;
using dRz.Abstractions.Services.Message;
using System;
using System.Runtime.CompilerServices;

namespace dRz.Infrastructure.Services.Message
{
    /// <summary>
    /// Универсальный сервис сообщений. Выбирает способ вывода в зависимости от состояния nanoCAD.
    /// Интерактивные запросы всегда показываются оконной реализацией.
    /// </summary>
    public sealed class MessageService : IMessageService, IMessagePromptService
    {
        private readonly ICommandLineMessageService _commandLine;
        private readonly IDocumentService _documentService;
        private readonly IWindowMessageService _window;

        public MessageService(
            ICommandLineMessageService commandLine,
            IWindowMessageService window,
            IDocumentService documentService)
        {
            _commandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        }

        private IMessageService Current =>
            _documentService.IsActive ? _commandLine : _window;

        public void ErrorMessage(Exception ex, string title = "", [CallerMemberName] string? caller = null) =>
            Current.ErrorMessage(ex, title, caller);

        public void ErrorMessage(string message, Exception? ex = null, string title = "", [CallerMemberName] string? caller = null) =>
            Current.ErrorMessage(message, ex, title, caller);

        public void InfoMessage(string message, string title = "", [CallerMemberName] string? caller = null) =>
            Current.InfoMessage(message, title, caller);

        public void WarningMessage(string message, string title = "", [CallerMemberName] string? caller = null) =>
            Current.WarningMessage(message, title, caller);

        public MessageResult AskYesNo(string message, string? title = null, [CallerMemberName] string? caller = null) =>
            _window.AskYesNo(message, title, caller);

        public MessageResult AskOkCancel(string message, string? title = null, [CallerMemberName] string? caller = null) =>
            _window.AskOkCancel(message, title, caller);

        public MessageResult AskRetryCancel(string message, string? title = null, [CallerMemberName] string? caller = null) =>
            _window.AskRetryCancel(message, title, caller);

        public MessageResult AskYesNoCancel(string message, string? title = null, [CallerMemberName] string? caller = null) =>
            _window.AskYesNoCancel(message, title, caller);

        public MessageResult AskAbortRetryIgnore(string message, string? title = null, [CallerMemberName] string? caller = null) =>
            _window.AskAbortRetryIgnore(message, title, caller);
    }
}