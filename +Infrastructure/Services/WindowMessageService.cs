using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using System;
using System.Runtime.CompilerServices;
using System.Windows;

//#if !TEST
//using HostMgd.ApplicationServices;
//using HostMgd.EditorInput;
//using Application = HostMgd.ApplicationServices.Application;
//#endif

namespace drz.Infrastructure.Services
{
    public class WindowMessageService : IMessageService, IWindowMessageService
    {
        public WindowMessageService(IApplicationInfo applicationInfo, IWindowHandleProvider handleProvider )
        {
            _applicationInfo = applicationInfo;

            _cadWindowHandle= handleProvider.Handle;

        }

        public void ConsoleMessage(string message, [CallerMemberName] string caller = null)
        {
//#if !TEST
            //Document doc = Application.DocumentManager.MdiActiveDocument;
            //if (doc == null)
            //{
            //    InfoMessage(message, caller);
            //    return;
            //}

            //Editor ed = doc.Editor;

            //ed.WriteMessage("\n" + (string.IsNullOrWhiteSpace(caller) ? "" : $"{caller} >> ") + message);
//#else
            InfoMessage(message, caller);
//#endif
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
            if (/*_applicationInfo.CadWindowHandle*/_cadWindowHandle != IntPtr.Zero)
            {
                SetForegroundWindow(/*_applicationInfo.CadWindowHandle*/_cadWindowHandle);
            }

            MessageBox.Show((string.IsNullOrWhiteSpace(caller) ? "" : $"{caller} >> ") + message,
                _applicationInfo.TitlePrefix + "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private IApplicationInfo _applicationInfo;

        private IntPtr _cadWindowHandle=IntPtr.Zero;

    }
}