using dRz.Updater.Services;
using Teigha.Runtime;

namespace dRz.NC.Commands
{
    /// <summary>
    /// ручной запуск проверки обновления
    /// </summary>
    public class UpdateCheck
    {
        /// <summary>
        /// Запускает ручную проверку обновлений
        /// </summary>
        [CommandMethod("drz_UpdateCheck", CommandFlags.Session)]
        public static void UpdateCheckRun()
        {
            UpdateAsync updateAsync = new UpdateAsync();
            _ = updateAsync.MainAsync(UpdateLaunchMode.Manual);
        }
    }
}