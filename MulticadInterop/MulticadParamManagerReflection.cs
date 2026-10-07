using System;
using System.Collections.Generic;
using System.Reflection;

namespace drz.MulticadInterop
{
    /// <summary>
    /// Предоставляет reflection-доступ к McParamManager
    /// без compile-time зависимости от конкретной версии Multicad.
    /// </summary>
    public static class MulticadParamManagerReflection
    {
        /// <summary>
        /// Возвращает все публичные методы McParamManager.
        /// </summary>
        public static MethodInfo[] GetMethods()
        {
            Type type = FindParamManagerType();

            if (type == null)
            {
                return Array.Empty<MethodInfo>();
            }

            return type.GetMethods();
        }

        /// <summary>
        /// Возвращает методы McParamManager с указанным именем.
        /// </summary>
        public static MethodInfo[] GetMethods(string methodName)
        {
            MethodInfo[] methods = GetMethods();
            List<MethodInfo> result = new List<MethodInfo>();

            foreach (MethodInfo method in methods)
            {
                if (method.Name == methodName)
                {
                    result.Add(method);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Находит тип McParamManager в загруженных сборках.
        /// Поддерживаются оба написания namespace, встречающиеся в версиях Multicad.
        /// </summary>
        private static Type FindParamManagerType()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type type =
                        assembly.GetType(
                            "Multicad.ApplicationServices.McParamManager",
                            false)
                        ?? assembly.GetType(
                            "Multicad.AplicationServices.McParamManager",
                            false);

                    if (type != null)
                    {
                        return type;
                    }
                }
                catch
                {
                    // Нативные и смешанные сборки могут не поддерживать GetType.
                    // Они не относятся к API Multicad и должны быть пропущены.
                }
            }

            return null;
        }
    }
}
