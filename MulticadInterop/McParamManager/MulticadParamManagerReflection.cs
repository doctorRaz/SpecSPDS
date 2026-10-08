using System.Reflection;

namespace drz.MulticadInterop.McParamManager
{
    /// <summary>
    /// Предоставляет reflection-доступ к McParamManager
    /// без compile-time зависимости от конкретной версии Multicad.
    /// </summary>
    public static class MulticadParamManagerReflection
    {
        private static readonly Type ParamManagerType =
            MulticadReflection.FindType(
                "Multicad.ApplicationServices.McParamManager",
                "Multicad.AplicationServices.McParamManager");

        public static MethodInfo[] GetMethods()
        {
            return ParamManagerType?.GetMethods() ?? Array.Empty<MethodInfo>();
        }

        public static MethodInfo[] GetMethods(string methodName)
        {
            return MulticadReflection.GetMethods(ParamManagerType, methodName);
        }

        public static MethodInfo FindMethod(string methodName, params Type[] parameterTypes)
        {
            return MulticadReflection.FindMethod(
                ParamManagerType, methodName, parameterTypes);
        }

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

        public static MethodInfo FindSetParam()
        {
            return FindMethod("SetParam", typeof(object), typeof(int));
        }

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
    }
}
