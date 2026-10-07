using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;

namespace drz.MulticadInterop
{
    /// <summary>
    /// Предоставляет API-обёртку над McParamManager
    /// без compile-time зависимости от конкретной версии Multicad.
    /// </summary>
    public static class MulticadParamManager
    {
        private static readonly MethodInfo CallOptionsFormMethod =
            MulticadParamManagerReflection.FindMethod(
                "CallOptions", typeof(string), typeof(object));

        private static readonly MethodInfo CallOptionsHandleMethod =
            MulticadParamManagerReflection.FindMethod(
                "CallOptions", typeof(string), typeof(IntPtr));

        private static readonly MethodInfo GetBoolParamMethod =
            MulticadParamManagerReflection.FindMethod(
                "GetBoolParam", typeof(int));

        private static readonly MethodInfo GetColorParamMethod =
            MulticadParamManagerReflection.FindMethod(
                "GetColorParam", typeof(int));

        private static readonly MethodInfo GetDoubleParamMethod =
            MulticadParamManagerReflection.FindMethod(
                "GetDoubleParam", typeof(int));

        private static readonly MethodInfo GetInt32ParamMethod =
            MulticadParamManagerReflection.FindMethod(
                "GetInt32Param", typeof(int));

        private static readonly MethodInfo GetInt64ParamMethod =
            MulticadParamManagerReflection.FindMethod(
                "GetInt64Param", typeof(int));

        private static readonly MethodInfo GetProfiledLayerNameMethod =
            MulticadParamManagerReflection.FindMethod(
                "GetProfiledLayerName", typeof(string));

        private static readonly MethodInfo GetStringParamMethod =
            MulticadParamManagerReflection.FindMethod(
                "GetStringParam", typeof(int));

        private static readonly MethodInfo SetParamMethod =
            MulticadParamManagerReflection.FindSetParam();

        private static readonly MethodInfo SetParamWithStandartsMethod =
            MulticadParamManagerReflection.FindSetParamWithStandarts();

        /// <summary>
        /// Вызов диалога настроек.
        /// </summary>
        public static void CallOptions(string helpIndexName, object sender)
        {
            InvokeVoid(CallOptionsFormMethod, helpIndexName, sender);
        }

        /// <summary>
        /// Вызов диалога настроек.
        /// </summary>
        public static void CallOptions(string helpIndexName, IntPtr handle)
        {
            InvokeVoid(CallOptionsHandleMethod, helpIndexName, handle);
        }

        public static bool GetBoolParam(int idParam)
        {
            return (bool)Invoke(GetBoolParamMethod, idParam);
        }

        public static Color GetColorParam(int idParam)
        {
            return (Color)Invoke(GetColorParamMethod, idParam);
        }

        public static double GetDoubleParam(int idParam)
        {
            return (double)Invoke(GetDoubleParamMethod, idParam);
        }

        /// <summary>
        /// Получение параметра с автоматическим приведением типа.
        /// </summary>
        public static int GetInt32Param(int idParam)
        {
            return (int)Invoke(GetInt32ParamMethod, idParam);
        }

        public static long GetInt64Param(int idParam)
        {
            return (long)Invoke(GetInt64ParamMethod, idParam);
        }

        /// <summary>
        /// Получение имени слоя, начинающегося с префикса текущего профиля.
        /// Если переданное имя слоя уже начинается с одного из префиксов, оно не изменяется.
        /// </summary>
        public static string GetProfiledLayerName(string layerName)
        {
            return (string)Invoke(GetProfiledLayerNameMethod, layerName);
        }

        public static string GetStringParam(int idParam)
        {
            return (string)Invoke(GetStringParamMethod, idParam);
        }

        public static bool SetParam(object param, int idParam)
        {
            return (bool)Invoke(SetParamMethod, param, idParam);
        }

        public static bool SetParam(object param, int idParam, Standarts std)
        {
            if (SetParamWithStandartsMethod == null)
            {
                throw new InvalidOperationException(
                    "McParamManager.SetParam(object, int, Standarts) не найден");
            }

            Type runtimeStandartsType =
                SetParamWithStandartsMethod.GetParameters()[2].ParameterType;

            object runtimeStandarts =
                Enum.ToObject(runtimeStandartsType, (int)std);

            return (bool)SetParamWithStandartsMethod.Invoke(
                null,
                new object[] { param, idParam, runtimeStandarts });
        }

        private static object Invoke(MethodInfo method, params object[] parameters)
        {
            if (method == null)
            {
                throw new InvalidOperationException("Метод McParamManager не найден");
            }

            return method.Invoke(null, parameters);
        }

        private static void InvokeVoid(MethodInfo method, params object[] parameters)
        {
            Invoke(method, parameters);
        }
    }

    /// <summary>
    /// Стандарты оформления Multicad.
    /// </summary>
    [Flags]
    public enum Standarts
    {
        GOST = 1,
        SPDS = 2,
        ISO = 4,
        ISO_MECH = 4,
        DIN = 8,
        CSN = 16,
        PN = 32,
        JUS = 64,
        GB = 128,
        NF = 256,
        IS = 512,
        ISO_ARCH = 1024,
        ALL_ARCH = 1026,
        ANSI = 2048,
        ALL_MECH = 3069,
        ALL_FOREIGN = 4092,
        ALL_NO_SPDS = 4093,
        ALL = 4095,
        CUSTOM = 32768
    }
}
