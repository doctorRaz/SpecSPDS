using drz.Abstractions.Services;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace drz.CadServices.Services
{
    /// <summary>
    /// Обёртка над McNotificator NanoCad для вывода сообщений в командную строку.
    /// Работает без подключения сборки Multicad через Reflection.
    /// </summary>
    public class McNotificatorMessageServise : IMessageService, IMcNotificatorMessageService
    {
        #region Private Fields

        private static readonly MethodInfo _createMessage = FindCreateMessage();

        #endregion Private Fields

        #region Public Methods

        public void ConsoleMessage(string message, [CallerMemberName] string caller = null)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Выводит сообщение в командную строку NanoCad.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <exception cref="InvalidOperationException">
        /// Выбрасывается если McNotificator.CreateMessage не найден в загруженных сборках.
        /// </exception>
        public static void WriteMessage(string message)
        {
            if (_createMessage == null)
            {
                throw new InvalidOperationException("McNotificator.CreateMessage не найден");
            }

            _createMessage.Invoke(null, new object[] { message });
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Ищет метод CreateMessage в загруженных сборках NanoCad.
        /// Поддерживает оба варианта пространства имён — до и начиная с версии 26.
        /// </summary>
        /// <returns>
        /// <see cref="MethodInfo"/> метода CreateMessage или <c>null</c> если не найден.
        /// </returns>
        private static MethodInfo FindCreateMessage()
        {

            Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            foreach (Assembly? assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type type =
                        assembly.GetType("Multicad.ApplicationServices.McNotificator")
                     ?? assembly.GetType("Multicad.AplicationServices.McNotificator");

                    if (type != null)
                    {
                        sw.Stop();
                        System.Diagnostics.Debug.WriteLine(
                            $"McNotificator найден за {sw.ElapsedMilliseconds} мс"
                        );

                        return type.GetMethod("CreateMessage", new[] { typeof(string) });
                    }
                }
                catch { } // Пропускаем нативные и смешанные сборки

            }

            sw.Stop();
            System.Diagnostics.Debug.WriteLine(
                $"McNotificator не найден, поиск занял {sw.ElapsedMilliseconds} мс");

            return null;
        }

        

        #endregion Private Methods
    }
}