using System;
using System.Collections.Generic;
using System.Reflection;

namespace drz.MulticadInterop
{
    /// <summary>
    /// Тип уведомления Multicad.
    /// Числовые значения соответствуют NotificationEnumMgd.
    /// </summary>
    public enum NotificationType
    {
        Info = 0,
        Warning = 1,
        Error = 4
    }

    /// <summary>
    /// Предоставляет reflection-доступ к перегрузкам McNotificator.CreateMessage
    /// без compile-time зависимости от конкретной версии Multicad.
    /// </summary>
    public static class MulticadNotificatorReflection
    {
        /// <summary>
        /// Получает все перегрузки McNotificator.CreateMessage
        /// из загруженных сборок Multicad.
        /// </summary>
        public static MethodInfo[] GetCreateMessageMethods()
        {
            Type type = FindNotificatorType();

            if (type == null)
            {
                return Array.Empty<MethodInfo>();
            }

            MethodInfo[] methods = type.GetMethods();
            List<MethodInfo> result = new List<MethodInfo>();

            foreach (MethodInfo method in methods)
            {
                if (method.Name == "CreateMessage")
                {
                    result.Add(method);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Находит перегрузку CreateMessage с текстом сообщения
        /// и параметром типа NotificationEnumMgd.
        /// </summary>
        public static MethodInfo FindCreateMessage(NotificationType type)
        {
            MethodInfo[] methods = GetCreateMessageMethods();

            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length != 2 ||
                    parameters[0].ParameterType != typeof(string) ||
                    !parameters[1].ParameterType.IsEnum)
                {
                    continue;
                }

                object enumValue;

                try
                {
                    enumValue = Enum.ToObject(parameters[1].ParameterType, (int)type);
                }
                catch
                {
                    continue;
                }

                if (Convert.ToInt32(enumValue) == (int)type)
                {
                    return method;
                }
            }

            return null;
        }

        private static Type FindNotificatorType()
        {
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

                    if (type != null)
                    {
                        return type;
                    }
                }
                catch
                {
                    // Пропускаем сборки, которые нельзя корректно отразить.
                }
            }

            return null;
        }
    }
}
