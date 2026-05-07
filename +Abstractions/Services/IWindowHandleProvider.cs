using System;

namespace drz.Abstractions.Services
{
    /// <summary>Указатель на окно кад </summary>
    public interface IWindowHandleProvider
    {
        /// <summary> Указатель на окно CAD </summary>
        IntPtr Handle { get; }
    }
}
