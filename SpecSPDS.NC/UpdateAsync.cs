using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Abstractions.Settings;
using dRz.Abstractions.Updater;
using dRz.Updater;
using dRz.Updater.Services;
using System;
using System.Threading.Tasks;

namespace dRz.NC
{
    /// <summary>
    /// Координирует проверку и установку обновлений аддона из контекста CAD-хоста.
    /// </summary>
    /// <remarks>
    /// Класс использует зарегистрированные сервисы аддона. Пользовательские сообщения и
    /// журналирование выполняются через эти сервисы, а запуск обновления не блокирует поток вызова.
    /// </remarks>
    public class UpdateAsync
    {
        /// <summary>
        /// Выполняет проверку обновлений и, в зависимости от режима запуска и настроек,
        /// передаёт управление установкой обновления менеджеру обновлений.
        /// После завершения сохраняет время проверки и её результат в настройках.
        /// </summary>
        /// <param name="launchMode">Режим запуска, определяющий поведение проверки и установки.</param>
        /// <returns>Задача, представляющая асинхронную операцию обновления.</returns>
        /// <remarks>
        /// Ошибки выполнения и сохранения состояния перехватываются и передаются в журнал
        /// и сервис пользовательских сообщений. Ошибка сохранения состояния не отменяет
        /// уже полученный результат обновления.
        /// </remarks>
        public async Task UpdateAsyncRun(UpdateLaunchMode launchMode)
        {
            try
            {
                IAddonSettings settings = AddOnCtx.Services.Get<IAddonSettings>();

                UpdateManager updateManager = new UpdateManager(AddOnCtx.Msg, AddOnCtx.MsgPrt, AddOnCtx.NLogFactory, settings.Updates);

                string updateUrl = AddOnCtx.AddOnInfo.RepositoryUrl + "/" +
                                                            AddOnCtx.AddOnInfo.Product +
                                                            "/releases/latest/download/";
#if DEBUG

                //updateUrl = $"http://127.0.0.1:8080/{AddOnCtx.AddOnInfo.Product}";
#endif
                _logger.Trace($"updateUrl: {updateUrl}");

                UpdateRequest request = new UpdateRequest
                {
                    CurrentVersion = AddOnCtx.AddOnInfo.InstalledVersion,

                    UpdateUrl = updateUrl,

                    PackageDirectory = AddOnCtx.AddOnInfo.PackageDirectory,

                    IsPackage = AddOnCtx.AddOnInfo.HasPackage,

                    IsNetworkPath = AddOnCtx.AddOnInfo.IsNetworkPath(),

                    LaunchMode = launchMode,
                };

                // Сохраняем результат, чтобы следующий запуск мог показать актуальное состояние.
                UpdateResult result = await updateManager.RunAsync(request);

                settings.Updates.LastCheck = DateTime.UtcNow;

                settings.Updates.LastResult = result;

                try
                {
                    settings.Save();
                }
                catch (Exception saveException)
                {
                    string msg = "Не удалось сохранить состояние обновления.";

                    // Ошибка сохранения состояния не отменяет результат уже выполненного обновления.
                    _logger?.Error(saveException, msg);

                    _msg.ErrorMessage(msg, saveException);
                }
            }
            catch (Exception ex)
            {
                string msg = "Продолжение не возможно";

                _logger?.Fatal(ex, msg);

                _msg.ErrorMessage(msg, ex);
            }
        }

        // Статические ссылки соответствуют общему контексту аддона и используются для диагностики ошибок.
        private static IDrzLogger? _logger = AddOnCtx.NLogFactory.GetLogger(typeof(UpdateAsync));

        private static IMessageService _msg = AddOnCtx.Msg;
    }
}