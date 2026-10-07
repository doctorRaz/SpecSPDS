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
        /// Находит метод McParamManager по имени и типам параметров.
        /// </summary>
        public static MethodInfo FindMethod(string methodName, params Type[] parameterTypes)
        {
            MethodInfo[] methods = GetMethods(methodName);

            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length != parameterTypes.Length)
                {
                    continue;
                }

                bool match = true;

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType != parameterTypes[i])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return method;
                }
            }

            return null;
        }

        /// <summary>
        /// Находит перегрузку CallOptions с Form без compile-time зависимости
        /// от System.Windows.Forms.
        /// </summary>
        public static MethodInfo FindCallOptionsForm()
        {
            MethodInfo[] methods = GetMethods("CallOptions");

            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length == 2 &&
                    parameters[0].ParameterType == typeof(string) &&
                    parameters[1].ParameterType.FullName == "System.Windows.Forms.Form")
                {
                    return method;
                }
            }

            return null;
        }

        /// <summary>
        /// Находит метод SetParam с сигнатурой (object, int).
        /// </summary>
        public static MethodInfo FindSetParam()
        {
            return FindMethod("SetParam", typeof(object), typeof(int));
        }

        /// <summary>
        /// Находит метод SetParam с сигнатурой (object, int, Standarts).
        /// </summary>
        public static MethodInfo FindSetParamWithStandarts()
        {
            MethodInfo[] methods = GetMethods("SetParam");

            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length == 3 &&
                    parameters[0].ParameterType == typeof(object) &&
                    parameters[1].ParameterType == typeof(int) &&
                    parameters[2].ParameterType.IsEnum &&
                    parameters[2].ParameterType.Name == "Standarts")
                {
                    return method;
                }
            }

            return null;
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
                }
            }

            return null;
        }
    }
}
