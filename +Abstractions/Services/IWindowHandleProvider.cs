using System;

namespace drz.Abstractions.Services
{
    public interface IWindowHandleProvider
    {
        /// <summary> Указатель на окно CAD </summary>
        IntPtr Handle { get; }
    }
}
