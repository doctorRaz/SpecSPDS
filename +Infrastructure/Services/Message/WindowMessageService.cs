using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using drz.Abstractions.Services.Message;
using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

namespace drz.Infrastructure.Services.Message
{
    public class WindowMessageService : IMessageService, IWindowMessageService
    {
        //наследуемся от IMessageService, добавляются методы ASK

        #region Private Fields

        private readonly IAddOnInfo _applicationInfo;

        private readonly IntPtr _cadWindowHandle = IntPtr.Zero;

        #endregion Private Fields

        #region Public Constructors

        public WindowMessageService(IAddOnInfo addOnInfo, IWindowHandleProvider handleProvider)
        {
            _applicationInfo = addOnInfo;

            _cadWindowHandle = handleProvider.Handle;
        }

        #endregion Public Constructors

        #region Public Methods

        public void ErrorMessage(Exception ex, [CallerMemberName] string? caller = null)
        {
            //throw new NotImplementedException();
            if (_cadWindowHandle != IntPtr.Zero)
            {
                SetForegroundWindow(_cadWindowHandle);
            }

            MessageBox.Show((string.IsNullOrWhiteSpace(caller) ? "" : $"{caller} >> ") + ex,
                _applicationInfo.ProductTitlePrefix + "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ErrorMessage(string message, Exception? ex = null, [CallerMemberName] string? caller = null)
        {
            //throw new NotImplementedException();
            MessageBox.Show((string.IsNullOrWhiteSpace(caller) ? "" : $"{caller} >> ") + ex + "\n" + message,
              _applicationInfo.ProductTitlePrefix + "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void InfoMessage(string message, [CallerMemberName] string? caller = null)
        {
            if (_cadWindowHandle != IntPtr.Zero)
            {
                SetForegroundWindow(_cadWindowHandle);
            }

            MessageBox.Show((string.IsNullOrWhiteSpace(caller) ? "" : $"{caller} >> ") + message,
                _applicationInfo.ProductTitlePrefix
                + "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void WarningMessage(string message, [CallerMemberName] string? caller = null)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods

        public MessageResult AskAbortRetryIgnore(string message, string title, [CallerMemberName] string? caller = null)
        {
            throw new NotImplementedException();
        }

        public MessageResult AskOkCancel(string message, string title, [CallerMemberName] string? caller = null)
        {
            throw new NotImplementedException();
        }

        public MessageResult AskRetryCancel(string message, string title, [CallerMemberName] string? caller = null)
        {
            throw new NotImplementedException();
        }

        public MessageResult AskYesNo(string message, string title, [CallerMemberName] string? caller = null)
        {
            throw new NotImplementedException();
        }

        public MessageResult AskYesNoCancel(string message, string title, [CallerMemberName] string? caller = null)
        {
            throw new NotImplementedException();
        }

        #region Private Methods

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBoxW(
                IntPtr hWnd,
                string lpText,
                string lpCaption,
                uint uType);
        #endregion Private Methods
    }
}