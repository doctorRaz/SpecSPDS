using System.Runtime.CompilerServices;

namespace drz.Abstractions.Services.Message
{
    /// <summary>Сервис сообщений, окно</summary>
    /// <seealso cref="IMessageService" />
    public interface IWindowMessageService : IMessageService
    {
        MessageResult AskOkCancel(string message, string title, [CallerMemberName] string caller = null);
        MessageResult AskAbortRetryIgnore(string message, string title, [CallerMemberName] string caller = null);
        MessageResult AskYesNoCancel(string message, string title, [CallerMemberName] string caller = null);
        MessageResult AskYesNo(string message, string title, [CallerMemberName] string caller = null);
        MessageResult AskRetryCancel(string message, string title, [CallerMemberName] string caller = null);
    }
}