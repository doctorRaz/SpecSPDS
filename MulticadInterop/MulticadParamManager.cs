using System;
using System.Reflection;

namespace drz.MulticadInterop
{
    /// <summary>
    /// Предоставляет API-обёртку над McParamManager
    /// без compile-time зависимости от конкретной версии Multicad.
    /// </summary>
    public static class MulticadParamManager
    {
        private static readonly MethodInfo SetParamMethod =
            MulticadParamManagerReflection.FindSetParam();

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
    }
}
