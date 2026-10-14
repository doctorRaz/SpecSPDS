global using AddOnCtx = dRz.Src.Infrastructure.AddOnContext;

//global using static dRz.Src.Infrastructure.AddOnContext;

using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Test.Console;
using dRz.Updater;
using dRz.Abstractions.Updater;
using dRz.Updater.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace dRz.SpecSPDS.Test
{
    /// <summary>
    /// Start
    /// </summary>
    public class Program
    {
        //[STAThread]
        private static async Task Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                ConteinerCtx.Run();

                _logger = AddOnCtx.NLogFactory.GetLogger<Program>();

                _logger?.Debug("Start debug updater");

                _msg.InfoMessage($"Start update");

                UpdateManager updateManager = new UpdateManager(AddOnCtx.Msg, AddOnCtx.MsgPrt, AddOnCtx.NLogFactory);

                updateManager.Cleanup(AddOnCtx.AddOnInfo.PackageDirectory);
                //return;
                string updateUrl = AddOnCtx.AddOnInfo.RepositoryUrl + "/" +
                    AddOnCtx.AddOnInfo.Product +
                    "/releases/latest/download/";

                updateUrl = $"http://127.0.0.1:8080/{AddOnCtx.AddOnInfo.Product}";

            update:

                UpdateMode mode = UpdateMode.Disabled;

                UpdateLaunchMode launchMode = UpdateLaunchMode.Manual;

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
                //goto update;
            }
            catch (Exception ex)
            {
                _logger?.Fatal(ex, "Продолжение не возможно");

                _msg.ErrorMessage("Продолжение не возможно", ex);
            }
            finally
            {
                _logger?.Info("Terminate");

                ConsoleReadKey(sw);
            }
        }

        private static ConsoleKeyInfo ConsoleReadKey(Stopwatch? sw = null)
        {
            string res = "";

            if (sw != null)
            {
                res = $"Total time {sw.Elapsed}\n";
            }

            _msg.InfoMessage($"{res}Press any key to exit...");

            return Console.ReadKey();
        }

        private static IDrzLogger? _logger = AddOnCtx.NLogFactory.GetLogger(typeof(Program));

        private static IMessageService _msg = AddOnCtx.Msg;
    }
}