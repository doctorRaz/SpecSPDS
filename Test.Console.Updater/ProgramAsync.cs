using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Abstractions.Updater;
using dRz.Updater;
using dRz.Updater.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace dRz.Test.Console.Updater
{
    public class ProgramAsync
    {
        public async Task MainAsync()
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                UpdateManager updateManager = new UpdateManager(AddOnCtx.Msg, AddOnCtx.MsgPrt, AddOnCtx.NLogFactory);

                string updateUrl = AddOnCtx.AddOnInfo.RepositoryUrl + "/" +
                                                            AddOnCtx.AddOnInfo.Product +
                                                            "/releases/latest/download/";

                updateUrl = $"http://127.0.0.1:8080/{AddOnCtx.AddOnInfo.Product}";

                UpdateMode mode = UpdateMode.Disabled;

                UpdateLaunchMode launchMode = UpdateLaunchMode.Automatic;

                UpdateRequest request = new UpdateRequest
                {
                    CurrentVersion = AddOnCtx.AddOnInfo.RunningVersion,

                    UpdateUrl = updateUrl,

                    Mode = mode,

                    PackageDirectory = AddOnCtx.AddOnInfo.PackageDirectory,

                    IsPackage = AddOnCtx.AddOnInfo.HasPackage,

                    IsNetworkPath = AddOnCtx.AddOnInfo.IsNetworkPath(),

                    LaunchMode = launchMode,
                };

                // Результат позволяет вызывающему коду различать исходы обновления.
                UpdateResult result = await updateManager.RunAsync(request);

                switch (result)
                {
                    case UpdateResult.Installed:
                        _msg.InfoMessage("Обновление установлено. Перезапустите аддон.");
                        break;

                    case UpdateResult.UpToDate:
                        _msg.InfoMessage("Установлена актуальная версия.");
                        break;

                    case UpdateResult.Declined:
                        _msg.InfoMessage("Пользователь отказался от необязательного обновления.");
                        break;

                    case UpdateResult.Disabled:
                        _msg.InfoMessage("Обновление отключено настройками.");
                        break;

                    case UpdateResult.NetworkPath:
                        _msg.InfoMessage("Аддон загружен из сетевого расположения.");
                        break;

                    case UpdateResult.Failed:
                        _msg.WarningMessage("Не удалось выполнить обновление. Подробности — в журнале.");
                        break;

                    case UpdateResult.Cancelled:
                        _msg.InfoMessage("Операция отменена через CancellationToken.");
                        break;
                }

                _msg.InfoMessage($"{nameof(ProgramAsync)}: {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                _logger?.Fatal(ex, "Продолжение не возможно");

                _msg.ErrorMessage("Продолжение не возможно", ex);
            }
        }

        private static IDrzLogger? _logger = AddOnCtx.NLogFactory.GetLogger(typeof(ProgramAsync));

        private static IMessageService _msg = AddOnCtx.Msg;
    }
}