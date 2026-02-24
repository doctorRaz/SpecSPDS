using System;
using System.Runtime.CompilerServices;

namespace dRz.SpecSPDS.nCad.Interfaces
{
    /// <summary> Сервис сообщений </summary>
    public interface IMessageService
    {
        /// <summary> Сообщение в консоль (када или еще чего - не сильно важно) </summary>
        /// <param name="message">Выводимое сообщение, без начальных и конечных переносов строк</param>
        /// <param name="caller">Вызывающий метода</param>
        void ConsoleMessage(string message, [CallerMemberName] string? caller = null);
        /// <summary> Информационное сообщение </summary>
        /// <param name="message">Выводимое сообщение, без начальных и конечных переносов строк</param>
        /// <param name="caller">Вызывающий метода</param>
        void InfoMessage(string message, [CallerMemberName] string? caller = null);
        /// <summary> Сообщение об ошибке </summary>
        /// <param name="message">Выводимое сообщение, без начальных и конечных переносов строк</param>
        /// <param name="caller">Вызывающий метода</param>
        void ErrorMessage(string message, [CallerMemberName] string? caller = null);
        /// <summary> Сообщение об исключении </summary>
        /// <param name="ex">Исключение</param>
        /// <param name="caller">Вызывающий метод</param>
        void ExceptionMessage(Exception ex, string message="", [CallerMemberName] string? caller = null);
    }
}
