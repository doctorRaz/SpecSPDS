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
    public class UpdateAsync
    {
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

                // Результат позволяет вызывающему коду различать исходы обновления.
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

        private static IDrzLogger? _logger = AddOnCtx.NLogFactory.GetLogger(typeof(UpdateAsync));

        private static IMessageService _msg = AddOnCtx.Msg;
    }
}