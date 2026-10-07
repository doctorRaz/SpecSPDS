using System;
using System.Collections.Generic;
using System.Reflection;

namespace drz.MulticadInterop
{
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
        /// <returns>
        /// Массив найденных перегрузок. Если McNotificator не найден,
        /// возвращается пустой массив.
        /// </returns>
        public static MethodInfo[] GetCreateMessageMethods()
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

                    if (type == null)
                    {
                        continue;
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
                catch
                {
                    // Пропускаем сборки, которые нельзя корректно отразить.
                }
            }

            return Array.Empty<MethodInfo>();
        }
    }
}
