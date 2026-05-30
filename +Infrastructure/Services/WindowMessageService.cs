using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using System;
using System.Runtime.CompilerServices;
using System.Windows;

namespace drz.Infrastructure.Services
{
    public class WindowMessageService : IMessageService, IWindowMessageService
    {

        #region Private Fields

        private IApplicationInfo _applicationInfo;

        private IntPtr _cadWindowHandle = IntPtr.Zero;

        #endregion Private Fields

        #region Public Constructors

        public WindowMessageService(IApplicationInfo applicationInfo, IWindowHandleProvider handleProvider)
        {
            _applicationInfo = applicationInfo;

            _cadWindowHandle = handleProvider.Handle;
        }

        #endregion Public Constructors

        #region Public Methods

        public void ConsoleMessage(string message, [CallerMemberName] string caller = null)
        {
            InfoMessage(message, caller);
        }

        public void ErrorMessage(string message, [CallerMemberName] string caller = null)
        {
            throw new NotImplementedException();
        }

        public void ExceptionMessage(Exception ex, [CallerMemberName] string caller = null)
        {
            throw new NotImplementedException();
        }

        public void ExceptionMessage(string message, Exception ex, [CallerMemberName] string caller = null)
        {
            throw new NotImplementedException();
        }

        public void InfoMessage(string message, [CallerMemberName] string caller = null)
        {
            if (_cadWindowHandle != IntPtr.Zero)
            {
                SetForegroundWindow(_cadWindowHandle);
            }

            MessageBox.Show((string.IsNullOrWhiteSpace(caller) ? "" : $"{caller} >> ") + message,
                _applicationInfo.TitlePrefix + "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion Public Methods

        #region Private Methods

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        #endregion Private Methods

    }
}