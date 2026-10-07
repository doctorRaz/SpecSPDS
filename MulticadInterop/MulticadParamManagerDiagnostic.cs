using System;
using System.Diagnostics;
using System.Reflection;

namespace drz.MulticadInterop
{
    /// <summary>
    /// Диагностика публичных методов McParamManager.
    /// </summary>
    public static class MulticadParamManagerDiagnostic
    {
        /// <summary>
        /// Выводит сигнатуры всех публичных методов McParamManager в Debug.
        /// </summary>
        public static void DumpMethods()
        {
            MethodInfo[] methods = MulticadParamManagerReflection.GetMethods();

            Debug.WriteLine($"McParamManager: найдено методов: {methods.Length}");

            foreach (MethodInfo method in methods)
            {
                Debug.WriteLine(FormatSignature(method));
            }
        }

        private static string FormatSignature(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            string signature = $"{method.ReturnType} {method.Name}(";

            for (int i = 0; i < parameters.Length; i++)
            {
                if (i > 0)
                {
                    signature += ", ";
                }

                ParameterInfo parameter = parameters[i];
                signature += $"{parameter.ParameterType} {parameter.Name}";
            }

            return signature + ")";
        }
    }
}
