using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Abstractions.Settings;
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
                IAddonSettings settings = AddOnCtx.Services.Get<IAddonSettings>();
                SelectUpdateMode(settings);

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
                    _logger?.Error(saveException, "Не удалось сохранить состояние обновления.");
                }

                _msg.InfoMessage($"{nameof(ProgramAsync)}: {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                _logger?.Fatal(ex, "Продолжение не возможно");

                _msg.ErrorMessage("Продолжение не возможно", ex);
            }
        }

        /// <summary>
        /// Позволяет вручную выбрать режим обновления для проверки в консольном тесте.
        /// </summary>
        private static void SelectUpdateMode(IAddonSettings settings)
        {
            while (true)
            {
                _msg.InfoMessage($"Текущий режим обновления: {settings.Updates.Mode}");
                _msg.InfoMessage("Выберите режим: 0 — Disabled, 1 — CheckAndNotify, 2 — CheckAndInstall.");
                _msg.InfoMessage("Enter — оставить текущий режим.");

                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    return;

                UpdateMode? selected = input.Trim() switch
                {
                    "0" => UpdateMode.Disabled,
                    "1" => UpdateMode.CheckAndNotify,
                    "2" => UpdateMode.CheckAndInstall,
                    _ => null
                };

                if (selected == null)
                {
                    _msg.InfoMessage("Неверный выбор. Введите 0, 1, 2 или Enter.");
                    continue;
                }

                UpdateMode previous = settings.Updates.Mode;
                settings.Updates.Mode = selected.Value;

                try
                {
                    settings.Save();
                    _msg.InfoMessage($"Режим обновления сохранён: {settings.Updates.Mode}");
                }
                catch (Exception ex)
                {
                    settings.Updates.Mode = previous;
                    _logger?.Error(ex, "Не удалось сохранить режим обновления.");
                    _msg.ErrorMessage("Не удалось сохранить режим. Оставлен прежний режим.", ex);
                }

                return;
            }
        }

        private static IDrzLogger? _logger = AddOnCtx.NLogFactory.GetLogger(typeof(ProgramAsync));

        private static IMessageService _msg = AddOnCtx.Msg;
    }
}