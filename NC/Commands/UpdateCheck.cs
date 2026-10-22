using dRz.NC.Infrastructure;
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
        [CommandMethod(CommandPrefix.Value + "_UpdateCheck", CommandFlags.Session)]
        public static void UpdateCheckRun()
        {
            //команд метод точка входа в приложение, поэтому можно использовать сервисы напрямую
            //создаем экземпляр класса UpdateAsync и запускаем метод UpdateAsyncRun с параметром UpdateLaunchMode.Manual
            UpdateAsync updateAsync = new UpdateAsync(AddOnContext.NLogFactory,
                                                        AddOnContext.Msg,
                                                        AddOnContext.MsgP);

            _ = updateAsync.UpdateAsyncRun(UpdateLaunchMode.Manual,
                                            AddOnContext.AddonSettings,
                                            AddOnContext.AddOnInfo);
        }
    }
}