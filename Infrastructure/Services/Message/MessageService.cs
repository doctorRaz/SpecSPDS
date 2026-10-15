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

        public void ErrorMessage(Exception ex, [CallerMemberName] string? caller = null, string? title = null) =>
            Current.ErrorMessage(ex, caller, title);

        public void ErrorMessage(string message, Exception? ex = null, [CallerMemberName] string? caller = null, string? title = null) =>
            Current.ErrorMessage(message, ex, caller, title);

        public void InfoMessage(string message, [CallerMemberName] string? caller = null, string? title = null) =>
            Current.InfoMessage(message, caller, title);

        public void WarningMessage(string message, [CallerMemberName] string? caller = null, string? title = null) =>
            Current.WarningMessage(message, caller, title);

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