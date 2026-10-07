using System.Diagnostics;
using System.Reflection;

namespace drz.MulticadInterop.McNotificator
{
    /// <summary>
    /// Диагностика reflection-доступа к McNotificator.CreateMessage.
    /// Используется для проверки реальных сигнатур и значений NotificationEnumMgd.
    /// </summary>
    public static class MulticadNotificatorDiagnostic
    {
        /// <summary>
        /// Выводит в Debug все перегрузки CreateMessage,
        /// параметры и значения enum параметра.
        /// </summary>
        public static void DumpCreateMessage()
        {
            MethodInfo[] methods = MulticadNotificatorReflection.GetCreateMessageMethods();

            foreach (MethodInfo method in methods)
            {
                Debug.WriteLine(method);

                foreach (ParameterInfo parameter in method.GetParameters())
                {
                    Debug.WriteLine(
                        $"  {parameter.ParameterType.FullName} {parameter.Name}");
                }

                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length > 1 && parameters[1].ParameterType.IsEnum)
                {
                    Type enumType = parameters[1].ParameterType;

                    Debug.WriteLine($"  Enum: {enumType.FullName}");

                    foreach (object value in Enum.GetValues(enumType))
                    {
                        Debug.WriteLine(
                            $"    {Enum.GetName(enumType, value)} = {Convert.ToInt32(value)}");
                    }
                }
            }
        }
    }
}
