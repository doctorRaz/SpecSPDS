using drz.Abstractions.Logger;

using drz.EnvironmentInfo;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.Src.Infrastructure
{
    /// <summary>
    /// Проброс в фабрику инфы о ОС КАД и аддоне
    /// </summary>
    /// <seealso cref="Abstractions.Logger.IEnvironmentInfoProvider" />
    public class CadEnvironmentInfoProvider : IEnvironmentInfoProvider
    {
        /// <summary>
        /// Gets the summary.
        /// </summary>
        /// <returns></returns>
        public string GetSummary()
        {
            //порнография конечно( /InfoDll/
            return $"{RT.Info.ToString()}\n{InfoDll.ToString()}";
        }
    }
}