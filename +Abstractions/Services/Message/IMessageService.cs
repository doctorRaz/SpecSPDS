using System;
using System.Runtime.CompilerServices;

namespace drz.Abstractions.Services.Message
{
    /// <summary> Общий сервис сообщений для пользователя </summary>
    public interface IMessageService
    {
        #region Public Methods

        /// <summary> Сообщение в консоль CAD </summary>
        /// <param name="message">Выводимое сообщение</param>
        /// <param name="callerName">Вызывающий метод. null - вычисляется автоматически. В случае, если ком.строка недоступна,<br/>
        /// рекомендуется реализовывать вызов <see cref="InfoMessage"/> с теми же параметрам </param>
        /// <example>Пример использования в C#
        /// <code language="cs">
        /// <![CDATA[
        /// IMessageService msgService = new MyMessageService();
        /// msgService.ConsoleMessage("Консольное сообщение");
        /// ]]>
        /// </code></example>
        /// <remarks>В зависимости от основной CAD-системы может понадобиться в реализации обрамлять сообщение символами перевода строки (\n)</remarks>
        void ConsoleMessage(string message, [CallerMemberName] string caller = null);

        /// <summary> Сообщение об ошибке, не вызывающей критическую остановку выполнения кода </summary>
        /// <param name="message">Выводимое сообщение</param>
        /// <param name="callerName">Вызывающий метод. null - вычисляется автоматически</param>
        /// <example>Пример использования в C#
        /// <code language="cs">
        /// <![CDATA[
        /// IMessageService msgService = new MyMessageService();
        /// msgService.ErrorMessage("Ошибка. Может быть, в аналог MessageBox");
        /// ]]>
        /// </code>
        /// </example>
        void ErrorMessage(string message, [CallerMemberName] string caller = null);

        /// <summary> Сообщение об исключении. Как правило, блокирует дальнейшее выполнение кода </summary>
        /// <param name="ex">Полное описание ошибки</param>
        /// <param name="callerName">Вызывающий метод. null - вычисляется автоматически</param>
        /// <example>Пример использования в C#
        /// <code language="cs">
        /// <![CDATA[
        /// IMessageService msgService = new MyMessageService();
        /// try
        /// {
        /// var res = (/ 50.0 0.0);
        /// }
        /// catch (Exception ex)
        /// {
        /// msgService.ExceptionMessage(ex);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        void ExceptionMessage(Exception ex, [CallerMemberName] string caller = null);

        /// <summary> Сообщение об исключении </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="caller">Вызывающий метод</param>
        void ExceptionMessage(string message, Exception ex, [CallerMemberName] string caller = null);

        /// <summary> Информационное сообщение для CAD </summary>
        /// <param name="message">Выводимое сообщение</param>
        /// <param name="callerName">Вызывающий метод. null - вычисляется автоматически</param>
        /// <example>Пример использования в C#
        /// <code language="cs">
        /// <![CDATA[
        /// IMessageService msgService = new MyMessageService();
        /// msgService.InfoMessage("Информационное сообщение. Может быть, в аналог MessageBox");
        /// ]]>
        /// </code>
        /// </example>
        void InfoMessage(string message, [CallerMemberName] string caller = null);

        #endregion Public Methods
    }
}