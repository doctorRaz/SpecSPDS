using System;
using System.Collections.Generic;
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
        private static readonly MethodInfo CreateMessageMethod = FindCreateMessage();
        private static readonly Dictionary<NotificationType, MethodInfo>
            CreateMessageMethods = FindCreateMessageMethods();
        private static readonly Dictionary<NotificationType, MethodInfo>
            CreateMessageWithParentMethods = FindCreateMessageWithParentMethods();

        /// <summary>
        /// Выводит сообщение в командную строку NanoCAD.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <exception cref="InvalidOperationException">
        /// Возникает, если McNotificator.CreateMessage не найден
        /// в загруженных сборках.
        /// </exception>
        [Obsolete ("use CreateMessage",false)]
        public static void WriteMessage(string message)
        {
            //https://learn.microsoft.com/ru-ru/dotnet/csharp/language-reference/attributes/general

            if (CreateMessageMethod == null)
            {
                throw new InvalidOperationException(
                    "McNotificator.CreateMessage не найден");
            }

            CreateMessageMethod.Invoke(null, new object[] { message });
        }

        /// <summary>
        /// Создаёт обычное уведомление.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <returns>Идентификатор созданного уведомления.</returns>
        public static UIntPtr CreateMessage(string message)
        {
            if (CreateMessageMethod == null)
            {
                throw new InvalidOperationException(
                    "McNotificator.CreateMessage не найден");
            }

            return (UIntPtr)CreateMessageMethod.Invoke(
                null,
                new object[] { message });
        }

        /// <summary>
        /// Создаёт уведомление указанного типа.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="type">Тип уведомления.</param>
        /// <returns>Идентификатор созданного уведомления.</returns>
        public static UIntPtr CreateMessage(string message, NotificationType type)
        {
            if (!CreateMessageMethods.TryGetValue(type, out MethodInfo method))
            {
                throw new InvalidOperationException(
                    "McNotificator.CreateMessage с типом уведомления не найден");
            }

            Type enumType = method.GetParameters()[1].ParameterType;
            object enumValue = Enum.ToObject(enumType, (int)type);

            return (UIntPtr)method.Invoke(
                null,
                new object[] { message, enumValue });
        }

        /// <summary>
        /// Создаёт уведомление указанного типа с идентификатором родительского уведомления.
        /// </summary>
        /// <param name="message">Текст сообщения.</param>
        /// <param name="type">Тип уведомления.</param>
        /// <param name="parentId">Идентификатор родительского уведомления.</param>
        /// <returns>Идентификатор созданного уведомления.</returns>
        public static UIntPtr CreateMessage(
            string message,
            NotificationType type,
            UIntPtr parentId)
        {
            if (!CreateMessageWithParentMethods.TryGetValue(
                    type,
                    out MethodInfo method))
            {
                throw new InvalidOperationException(
                    "McNotificator.CreateMessage с parentId не найден");
            }

            Type enumType = method.GetParameters()[1].ParameterType;
            object enumValue = Enum.ToObject(enumType, (int)type);

            return (UIntPtr)method.Invoke(
                null,
                new object[] { message, enumValue, parentId });
        }

        /// <summary>
        /// Кэширует перегрузки CreateMessage для поддерживаемых типов уведомлений.
        /// </summary>
        private static Dictionary<NotificationType, MethodInfo> FindCreateMessageMethods()
        {
            Dictionary<NotificationType, MethodInfo> result =
                new Dictionary<NotificationType, MethodInfo>();

            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                MethodInfo method =
                    MulticadNotificatorReflection.FindCreateMessage(type);

                if (method != null)
                {
                    result[type] = method;
                }
            }

            return result;
        }

        /// <summary>
        /// Кэширует перегрузки CreateMessage с parentId для поддерживаемых типов уведомлений.
        /// </summary>
        private static Dictionary<NotificationType, MethodInfo>
            FindCreateMessageWithParentMethods()
        {
            Dictionary<NotificationType, MethodInfo> result =
                new Dictionary<NotificationType, MethodInfo>();

            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                MethodInfo method =
                    MulticadNotificatorReflection.FindCreateMessage(type, true);

                if (method != null)
                {
                    result[type] = method;
                }
            }

            return result;
        }

        /// <summary>
        /// Находит однопараметрическую перегрузку McNotificator.CreateMessage.
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
