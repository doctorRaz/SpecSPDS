using System;
using System.Collections.Generic;
using System.Reflection;

namespace drz.MulticadInterop.McNotificator
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
        private static readonly Type NotificatorType =
            MulticadReflection.FindType(
                "Multicad.ApplicationServices.McNotificator",
                "Multicad.AplicationServices.McNotificator");

        public static MethodInfo[] GetCreateMessageMethods()
        {
            return MulticadReflection.GetMethods(
                NotificatorType, "CreateMessage");
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
    }
}
