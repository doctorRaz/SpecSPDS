using System;
using System.Diagnostics;
using System.Reflection;
//todo все связанное с HostMgd в отдельную сборку CadInfrastructure
namespace drz.Infrastructure.Services
{
    /// <summary>
    /// Обёртка над McNotificator NanoCad для вывода сообщений в командную строку.
    /// Работает без подключения сборки Multicad через Reflection.
    /// </summary>
    public static class McNotificator
    {
        private static readonly MethodInfo _createMessage = FindCreateMessage();

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
    }
}