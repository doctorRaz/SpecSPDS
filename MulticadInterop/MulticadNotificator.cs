using System;
using System.Diagnostics;
using System.Reflection;

namespace drz.MulticadInterop
{
    /// <summary>
    /// Предоставляет доступ к McNotificator без compile-time зависимости
    /// от конкретной версии Multicad.
    /// </summary>
    public static class MulticadNotificator
    {
        /// <summary>
        /// Выводит сообщение в командную строку NanoCAD.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <exception cref="InvalidOperationException">
        /// Возникает, если McNotificator.CreateMessage не найден
        /// в загруженных сборках.
        /// </exception>
        public static void WriteMessage(string message)
        {
            if (CreateMessageMethod == null)
            {
                throw new InvalidOperationException(
                    "McNotificator.CreateMessage не найден");
            }

            CreateMessageMethod.Invoke(null, new object[] { message });
        }

        private static readonly MethodInfo CreateMessageMethod = FindCreateMessage();

        /// <summary>
        /// Находит метод McNotificator.CreateMessage в загруженных сборках.
        /// Поддерживаются оба написания namespace, встречающиеся в версиях Multicad.
        /// </summary>
        private static MethodInfo FindCreateMessage()
        {
            Stopwatch sw = Stopwatch.StartNew();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type type =
                        assembly.GetType(
                            "Multicad.ApplicationServices.McNotificator",
                            false)
                        ?? assembly.GetType(
                            "Multicad.AplicationServices.McNotificator",
                            false);

                    if (type == null)
                    {
                        continue;
                    }

                    MethodInfo method = type.GetMethod(
                        "CreateMessage",
                        new[] { typeof(string) });

                    if (method != null)
                    {
                        sw.Stop();
                        Debug.WriteLine(
                            $"McNotificator.CreateMessage найден за {sw.ElapsedMilliseconds} мс");
                        return method;
                    }
                }
                catch
                {
                    // Нативные и смешанные сборки могут не поддерживать GetType.
                    // Они не относятся к API Multicad и должны быть пропущены.
                }
            }

            sw.Stop();
            Debug.WriteLine(
                $"McNotificator.CreateMessage не найден, поиск занял {sw.ElapsedMilliseconds} мс");

            return null;
        }
    }
}