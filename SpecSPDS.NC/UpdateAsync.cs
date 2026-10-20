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
                IUpdateSettings updateSettings = settings.Updates;

                int minDay = 1;

#if DEBUG
                minDay = 0;//интервал для отладки
#endif

                // Ограничиваем интервал автоматических проверок допустимым диапазоном.
                int configuredInterval = updateSettings.UpdateCheckIntervalDays;
                int normalizedInterval = Math.Clamp(configuredInterval, minDay, 30);
                bool intervalNormalized = configuredInterval != normalizedInterval;

                if (intervalNormalized)
                {
                    updateSettings.UpdateCheckIntervalDays = normalizedInterval;
                    try
                    {
                        // Сохраняем нормализованное значение даже если проверка будет пропущена.
                        settings.Save();
                    }
                    catch (Exception saveException)
                    {
                        ReportError(saveException, "Не удалось сохранить интервал проверки обновлений.");
                    }
                }

                // Автоматическая проверка выполняется не чаще заданного интервала.
                // Ручной запуск всегда игнорирует ограничение.
                if (launchMode == UpdateLaunchMode.Automatic &&
                    updateSettings.LastCheck.HasValue &&
                    DateTime.UtcNow - updateSettings.LastCheck.Value < TimeSpan.FromDays(normalizedInterval))
                {
                    _logger.Trace(
                        $"Автоматическая проверка пропущена: с последней успешной проверки " +
                        $"не прошло {normalizedInterval} дн.");
                    return;
                }

                string updateUrl = AddOnCtx.AddOnInfo.RepositoryUrl + "/" +
                                   AddOnCtx.AddOnInfo.Product +
                                   "/releases/latest/download/";
#if DEBUG
                updateUrl = $"http://127.0.0.1:8080/{AddOnCtx.AddOnInfo.Product}/{AddOnCtx.AddOnInfo.HostFamily}";
#endif
                _logger.Trace($"updateUrl: {updateUrl}");

                UpdateManager updateManager = new UpdateManager(
                    AddOnCtx.Msg,
                    AddOnCtx.MsgPrt,
                    AddOnCtx.NLogFactory,
                    updateSettings);

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

                // Обновляем LastCheck только если проверка/операция завершилась успешно.
                // Ошибка, отмена и блокировка из-за сетевого расположения интервал не сбрасывают.
                if (result is UpdateResult.Installed
                    or UpdateResult.UpToDate
                    or UpdateResult.Declined
                    or UpdateResult.Disabled)
                {
                    updateSettings.LastCheck = DateTime.UtcNow;
                }

                // Сохраняем результат фактически выполненного запуска.
                updateSettings.LastResult = result;

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