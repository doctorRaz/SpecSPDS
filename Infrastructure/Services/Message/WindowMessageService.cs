using dRz.Abstractions.Infrastructure;
using dRz.Abstractions.Services.Message;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

namespace dRz.Infrastructure.Services.Message
{
    /// <summary>Оконная реализация обычных и интерактивных сообщений.</summary>
    public class WindowMessageService : IWindowMessageService
    {
        private readonly string _productTitlePrefix;
        private readonly IntPtr _cadWindowHandle;

        public WindowMessageService(IAddOnInfo addOnInfo, IWindowHandleProvider handleProvider)
        {
            _productTitlePrefix = addOnInfo.ProductTitlePrefix;
            _cadWindowHandle = handleProvider.Handle;
        }

        public void ErrorMessage(Exception ex, string title = "", [CallerMemberName] string? caller = null)
        {
            ActivateCadWindow();
            MessageBox.Show(FormatMessage(ex.ToString(), caller),
                BuildTitle(title, "Error"), MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ErrorMessage(string message, Exception? ex = null, string title = "", [CallerMemberName] string? caller = null)
        {
            ActivateCadWindow();
            string text = ex == null ? message : $"{ex}\n{message}";
            MessageBox.Show(FormatMessage(text, caller),
                BuildTitle(title, "Error"), MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void InfoMessage(string message, string title = "", [CallerMemberName] string? caller = null)
        {
            ActivateCadWindow();
            MessageBox.Show(FormatMessage(message, caller),
                BuildTitle(title, "Info"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void WarningMessage(string message, string title = "", [CallerMemberName] string? caller = null)
        {
            ActivateCadWindow();
            MessageBox.Show(FormatMessage(message, caller),
                BuildTitle(title, "Warn"), MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public MessageResult AskAbortRetryIgnore(string message, string title = "", [CallerMemberName] string? caller = null) =>
            ShowPrompt(message, title, caller, MessageBoxButton.AbortRetryIgnore, MessageBoxImage.Warning);

        public MessageResult AskOkCancel(string message, string? title = null, [CallerMemberName] string? caller = null) =>
            ShowPrompt(message, title, caller, MessageBoxButton.OKCancel, MessageBoxImage.Question);

        public MessageResult AskRetryCancel(string message, string? title = null, [CallerMemberName] string? caller = null) =>
            ShowPrompt(message, title, caller, MessageBoxButton.RetryCancel, MessageBoxImage.Question);

        public MessageResult AskYesNo(string message, string? title = null, [CallerMemberName] string? caller = null) =>
            ShowPrompt(message, title, caller, MessageBoxButton.YesNo, MessageBoxImage.Question);

        public MessageResult AskYesNoCancel(string message, string? title = null, [CallerMemberName] string? caller = null) =>
            ShowPrompt(message, title, caller, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

        private MessageResult ShowPrompt(string message, string? title, string? caller, MessageBoxButton buttons, MessageBoxImage image)
        {
            ActivateCadWindow();
            MessageBoxResult result = MessageBox.Show(
                FormatMessage(message, caller),
                BuildTitle(title, null),
                buttons,
                image);

            return result switch
            {
                MessageBoxResult.OK => MessageResult.Ok,
                MessageBoxResult.Cancel => MessageResult.Cancel,
                MessageBoxResult.Yes => MessageResult.Yes,
                MessageBoxResult.No => MessageResult.No,
                MessageBoxResult.Retry => MessageResult.Retry,
                MessageBoxResult.Abort => MessageResult.Abort,
                MessageBoxResult.Ignore => MessageResult.Ignore,
                _ => MessageResult.None
            };
        }

        private string BuildTitle(string? title, string? category)
        {
            if (!string.IsNullOrWhiteSpace(title))
                return $"{_productTitlePrefix}: {title}";

            return _productTitlePrefix + (category ?? string.Empty);
        }

        private static string FormatMessage(string message, string? caller) =>
            (string.IsNullOrWhiteSpace(caller) ? string.Empty : $"{caller} >> ") + message;

        private void ActivateCadWindow()
        {
            if (_cadWindowHandle != IntPtr.Zero)
                SetForegroundWindow(_cadWindowHandle);
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}