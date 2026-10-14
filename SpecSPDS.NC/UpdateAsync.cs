using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Abstractions.Settings;
using dRz.Abstractions.Updater;
using dRz.Updater;
using dRz.Updater.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace dRz.NC
{
    public class UpdateAsync
    {
        public async Task MainAsync()
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                IAddonSettings settings = AddOnCtx.Services.Get<IAddonSettings>();
                UpdateManager updateManager = new UpdateManager(AddOnCtx.Msg, AddOnCtx.MsgPrt, AddOnCtx.NLogFactory, settings.Updates);

                string updateUrl = AddOnCtx.AddOnInfo.RepositoryUrl + "/" +
                                                            AddOnCtx.AddOnInfo.Product +
                                                            "/releases/latest/download/";

                updateUrl = $"http://127.0.0.1:8080/{AddOnCtx.AddOnInfo.Product}";

                UpdateLaunchMode launchMode = UpdateLaunchMode.Automatic;

                UpdateRequest request = new UpdateRequest
                {
                    CurrentVersion = AddOnCtx.AddOnInfo.RunningVersion,

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
                    // Ошибка сохранения состояния не отменяет результат уже выполненного обновления.
                    _logger?.Error(saveException, "Не удалось сохранить состояние обновления.");
                }

                _msg.InfoMessage($"{nameof(UpdateAsync)}: {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                _logger?.Fatal(ex, "Продолжение не возможно");

                _msg.ErrorMessage("Продолжение не возможно", ex);
            }
        }

        private static IDrzLogger? _logger = AddOnCtx.NLogFactory.GetLogger(typeof(UpdateAsync));

        private static IMessageService _msg = AddOnCtx.Msg;
    }
}