using System.Diagnostics;
using System.Reflection;

namespace drz.MulticadInterop
{
    /// <summary>
    /// Предоставляет доступ к API Multicad без compile-time зависимости
    /// от конкретной версии Multicad.
    /// </summary>
    public static class MulticadParamManager
    {
        private static readonly MethodInfo SetParamMethod = FindSetParam();

        /// <summary>
        /// Устанавливает параметр Multicad.
        /// </summary>
        /// <param name="value">Значение параметра.</param>
        /// <param name="parameter">Идентификатор параметра Multicad.</param>
        /// <exception cref="InvalidOperationException">
        /// Возникает, если API Multicad не найден в загруженных сборках.
        /// </exception>
        public static void SetParam(string value, int parameter)
        {
            if (SetParamMethod == null)
            {
                throw new InvalidOperationException("McParamManager.SetParam не найден");
            }

            object param = value;
            SetParamMethod.Invoke(null, new object[] { param, parameter });
        }

        /// <summary>
        /// Находит метод McParamManager.SetParam в загруженных сборках.
        /// Поддерживаются оба написания namespace, встречающиеся в версиях Multicad.
        /// </summary>
        private static MethodInfo FindSetParam()
        {
            Stopwatch sw = Stopwatch.StartNew();

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

                    if (type == null)
                    {
                        continue;
                    }

                    MethodInfo method = type.GetMethod(
                        "SetParam",
                        new[] { typeof(object).MakeByRefType(), typeof(int) });

                    if (method != null)
                    {
                        sw.Stop();
                        Debug.WriteLine(
                            $"McParamManager.SetParam найден за {sw.ElapsedMilliseconds} мс");
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
                $"McParamManager.SetParam не найден, поиск занял {sw.ElapsedMilliseconds} мс");

            return null;
        }
    }
}