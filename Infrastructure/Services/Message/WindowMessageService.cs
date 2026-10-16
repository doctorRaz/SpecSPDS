using dRz.Abstractions.Infrastructure;
using dRz.Abstractions.Services;
using dRz.Abstractions.Services.Message;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using Forms = System.Windows.Forms;

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
                BuildTitle(title), MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ErrorMessage(string message, Exception? ex = null, string title = "", [CallerMemberName] string? caller = null)
        {
            ActivateCadWindow();
            string text = ex == null ? message : $"{ex}\n{message}";
            MessageBox.Show(FormatMessage(text, caller),
                BuildTitle(title), MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void InfoMessage(string message, string title = "", [CallerMemberName] string? caller = null)
        {
            ActivateCadWindow();
            MessageBox.Show(FormatMessage(message, caller),
                BuildTitle(title), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void WarningMessage(string message, string title = "", [CallerMemberName] string? caller = null)
        {
            ActivateCadWindow();
            MessageBox.Show(FormatMessage(message, caller),
                BuildTitle(title), MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public MessageResult AskAbortRetryIgnore(string message, string title = "", [CallerMemberName] string? caller = null) => ShowPrompt(message, title, caller, Forms.MessageBoxButtons.AbortRetryIgnore, Forms.MessageBoxIcon.Warning);

        public MessageResult AskOkCancel(string message, string title = "", [CallerMemberName] string? caller = null) => ShowPrompt(message, title, caller, Forms.MessageBoxButtons.OKCancel, Forms.MessageBoxIcon.Question);

        public MessageResult AskRetryCancel(string message, string title = "", [CallerMemberName] string? caller = null) => ShowPrompt(message, title, caller, Forms.MessageBoxButtons.RetryCancel, Forms.MessageBoxIcon.Question);

        public MessageResult AskYesNo(string message, string title = "", [CallerMemberName] string? caller = null) => ShowPrompt(message, title, caller, Forms.MessageBoxButtons.YesNo, Forms.MessageBoxIcon.Question);

        public MessageResult AskYesNoCancel(string message, string title = "", [CallerMemberName] string? caller = null) => ShowPrompt(message, title, caller, Forms.MessageBoxButtons.YesNoCancel, Forms.MessageBoxIcon.Question);

        private MessageResult ShowPrompt(
            string message,
            string? title,
            string? caller,
            Forms.MessageBoxButtons buttons,
            Forms.MessageBoxIcon icon)
        {
            ActivateCadWindow();

            Forms.DialogResult result = _cadWindowHandle == IntPtr.Zero
                ? Forms.MessageBox.Show(
                    FormatMessage(message, caller),
                    BuildTitle(title),
                    buttons,
                    icon)
                : Forms.MessageBox.Show(
                    new WindowOwner(_cadWindowHandle),
                    FormatMessage(message, caller),
                    BuildTitle(title),
                    buttons,
                    icon);

            return result switch
            {
                Forms.DialogResult.OK => MessageResult.Ok,
                Forms.DialogResult.Cancel => MessageResult.Cancel,
                Forms.DialogResult.Yes => MessageResult.Yes,
                Forms.DialogResult.No => MessageResult.No,
                Forms.DialogResult.Retry => MessageResult.Retry,
                Forms.DialogResult.Abort => MessageResult.Abort,
                Forms.DialogResult.Ignore => MessageResult.Ignore,
                _ => MessageResult.None
            };
        }

        private string BuildTitle(string? title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                return $"{_productTitlePrefix}: {title}";
            }
            else
            {
                return $"{_productTitlePrefix}";
            }
        }

        private static string FormatMessage(string message, string? caller)
        {
            string _caller = "";
#if DEBUG
            _caller = $"{caller} >> ";
#endif
            return _caller + message;
        }

        private void ActivateCadWindow()
        {
            if (_cadWindowHandle != IntPtr.Zero)
            {
                SetForegroundWindow(_cadWindowHandle);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>Предоставляет WinForms-диалогу HWND окна nanoCAD в качестве владельца.</summary>
        private sealed class WindowOwner : Forms.IWin32Window
        {
            public WindowOwner(IntPtr handle) => Handle = handle;

            public IntPtr Handle { get; }
        }
    }
}