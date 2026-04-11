using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using System;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Test.Services
{
    public class WindowMessageService : IMessageService
    {
        public WindowMessageService(IApplicationInfo applicationInfo)
        {
            _applicationInfo = applicationInfo;
        }

        public void ConsoleMessage(string message, [CallerMemberName] string caller = null)
        {
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            //if (doc == null)
            //{
                InfoMessage(message, caller);
                return;
            //}

            //Editor ed = doc.Editor;
            //ed.WriteMessage("\n" + (string.IsNullOrWhiteSpace(caller) ? "" : $"{caller} >> ") + message);
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
            if (_applicationInfo.CadWindowHandle != IntPtr.Zero)
            {
                SetForegroundWindow(_applicationInfo.CadWindowHandle);
            }

            MessageBox.Show((string.IsNullOrWhiteSpace(caller) ? "" : $"{caller} >> ") + message,
                _applicationInfo.TitlePrefix + "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private IApplicationInfo _applicationInfo;
    }
}
