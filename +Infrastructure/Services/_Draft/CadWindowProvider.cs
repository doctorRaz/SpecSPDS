using drz.Abstractions.Services;
using System;
using System.Diagnostics;

#if !TEST
using HostMgd.ApplicationServices;
#endif

namespace drz.Infrastructure.Services
{

    /// <summary>
    /// Указатель на окно привязан к HostMgd <br/>
    /// Не используется
    /// </summary>
    /// <seealso cref="drz.Abstractions.Services.IWindowHandleProvider" />
    public class CadWindowProvider_Cad : IWindowHandleProvider
    {
#if !TEST
        public IntPtr Handle => Application.MainWindow.Handle;
#else
        public IntPtr Handle => IntPtr.Zero;
#endif
    }

    /// <summary>
    /// Указатель на окно отвязан от Cad
    /// </summary>
    /// <seealso cref="drz.Abstractions.Services.IWindowHandleProvider" />
    public class CadWindowProvider : IWindowHandleProvider
    {
        private IntPtr? _handle;

        public IntPtr Handle
        {
            get
            {
                // Кэшируем, чтобы не опрашивать процесс постоянно
                if (!_handle.HasValue || _handle.Value == IntPtr.Zero)
                {
                    // Получаем хэндл главного окна текущего запущенного процесса
                    _handle = Process.GetCurrentProcess().MainWindowHandle;
                }
                return _handle.Value;
            }
        }
    }
}
