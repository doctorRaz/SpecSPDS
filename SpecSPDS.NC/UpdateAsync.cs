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
    /// Класс использует зарегистрированные сервисы аддона. Асинхронный запуск не блокирует
    /// поток вызова; после асинхронных операций продолжение использует захваченный контекст,
    /// если он предоставлен вызывающей средой.
    /// </remarks>
    public class UpdateAsync
    {
        private readonly IDrzLogger _logger;
        private readonly IMessageService _msg;

        public UpdateAsync()
        {
            _logger = AddOnCtx.NLogFactory.GetLogger(typeof(UpdateAsync));
            _msg = AddOnCtx.Msg;
        }

        /// <summary>
        /// Выполняет проверку обновлений и, в зависимости от режима запуска и настроек,
        /// передаёт управление установкой обновления менеджеру обновлений.
        /// Сохраняет результат запуска и время завершённой проверки.
        /// </summary>
        /// <param name="launchMode">Режим запуска, определяющий поведение проверки и установки.</param>
        /// <returns>Задача, представляющая асинхронную операцию обновления.</returns>
        /// <remarks>
        /// Ошибки выполнения, сохранения состояния и уведомления об ошибках обрабатываются
        /// отдельно, чтобы сбой диагностики не маскировал исходную ошибку.
        /// </remarks>
        public async Task UpdateAsyncRun(UpdateLaunchMode launchMode)
        {
            try
            {
                IAddonSettings settings = AddOnCtx.Services.Get<IAddonSettings>();

                UpdateManager updateManager = new UpdateManager(
                    AddOnCtx.Msg,
                    AddOnCtx.MsgPrt,
                    AddOnCtx.NLogFactory,
                    settings.Updates);

                string updateUrl = AddOnCtx.AddOnInfo.RepositoryUrl + "/" +
                                   AddOnCtx.AddOnInfo.Product +
                                   "/releases/latest/download/";
#if DEBUG
                updateUrl = $"http://127.0.0.1:8080/{AddOnCtx.AddOnInfo.Product}/{AddOnCtx.AddOnInfo.HostFamily}";
#endif
                _logger.Trace($"updateUrl: {updateUrl}");

                UpdateRequest request = new UpdateRequest
                {
                    CurrentVersion = AddOnCtx.AddOnInfo.InstalledVersion,
                    UpdateUrl = updateUrl,
                    PackageDirectory = AddOnCtx.AddOnInfo.PackageDirectory,
                    IsPackage = AddOnCtx.AddOnInfo.IsProductDirectory,
                    IsNetworkPath = AddOnCtx.AddOnInfo.IsNetworkPath(),
                    LaunchMode = launchMode,
                };

                UpdateResult result = await updateManager.RunAsync(request);

                // Сохраняем время завершённой попытки, в том числе неудачной проверки.
                // При отмене или сетевом расположении проверка обновлений не завершена.
                if (result is not UpdateResult.Cancelled and not UpdateResult.NetworkPath)
                {
                    settings.Updates.LastCheck = DateTime.UtcNow;
                }

                // Результат сохраняется и для неуспешного запуска, чтобы его можно было диагностировать.
                settings.Updates.LastResult = result;

                try
                {
                    settings.Save();
                }
                catch (Exception saveException)
                {
                    ReportError(saveException, "Не удалось сохранить состояние обновления.");
                }
            }
            catch (Exception ex)
            {
                ReportError(ex, "Не удалось выполнить проверку или обновление.");
            }
        }

        /// <summary>
        /// Пытается записать ошибку в журнал и уведомить пользователя.
        /// Сбой одного из каналов диагностики не должен скрывать исходную ошибку.
        /// </summary>
        private void ReportError(Exception exception, string message)
        {
            try
            {
                _logger.Error(exception, message);
            }
            catch
            {
                // Не допускаем, чтобы сбой логгера прервал обработку исходной ошибки.
            }

            try
            {
                _msg.ErrorMessage(message, exception);
            }
            catch (Exception notificationException)
            {
                try
                {
                    _logger.Error(notificationException, "Не удалось показать сообщение об ошибке.");
                }
                catch
                {
                    // Оба канала диагностики могут быть недоступны.
                }
            }
        }
    }
}