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
        neSimple = -1,
        neUsual = 0,
        neWarning = 1,
        neCure = 2,
        neHint = 3,
        neError = 4,
        neHelp = 5
    }

    /// <summary>
    /// Предоставляет reflection-доступ к перегрузкам McNotificator.CreateMessage
    /// без compile-time зависимости от конкретной версии Multicad.
    /// </summary>
    public static class MulticadNotificatorReflection
    {
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

        public static MethodInfo FindCreateMessage(NotificationType type)
        {
            return FindCreateMessage(type, false);
        }

        public static MethodInfo FindCreateMessage(NotificationType type, bool withParentId)
        {
            MethodInfo[] methods = GetCreateMessageMethods();

            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();

                int expectedCount = withParentId ? 3 : 2;

                if (parameters.Length != expectedCount ||
                    parameters[0].ParameterType != typeof(string) ||
                    !parameters[1].ParameterType.IsEnum)
                {
                    continue;
                }

                if (withParentId && parameters[2].ParameterType != typeof(UIntPtr))
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
