global using AddOnCtx = dRz.Src.Infrastructure.AddOnContext;

//global using static dRz.Src.Infrastructure.AddOnContext;

using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Abstractions.Settings;
using dRz.NC;
using dRz.Updater.Services;
using System;
using System.Diagnostics;
using Sys = System;

namespace dRz.Test.Console.Updater
{
    public class Program
    {
        //[STAThread]
        private static void /*async Task*/ Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                ConteinerCtx.Run();

                _logger = AddOnCtx.NLogFactory.GetLogger<Program>();

                _logger?.Debug("Start debug updater");

                _msg.InfoMessage($"Start update");


                Program program = new Program();
                IAddonSettings settings = AddOnCtx.Services.Get<IAddonSettings>();
                program.SelectUpdateMode(settings);

                UpdateAsync updateAsync = new UpdateAsync();

                _ = updateAsync.UpdateAsyncRun(UpdateLaunchMode.Automatic);
            }
            catch (Exception ex)
            {
                _logger?.Fatal(ex, "Продолжение не возможно");

                _msg.ErrorMessage("Продолжение не возможно", ex);
            }
            finally
            {
                _logger?.Info("Terminate");

                _ = ConsoleReadKey(sw);
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

            return Sys.Console.ReadKey();
        }

        private static IDrzLogger? _logger = AddOnCtx.NLogFactory.GetLogger(typeof(Program));

        private static IMessageService _msg = AddOnCtx.Msg;

        /// <summary>
        /// Позволяет вручную выбрать режим обновления для проверки в консольном тесте.
        /// </summary>
        private void SelectUpdateMode(IAddonSettings settings)
        {
            while (true)
            {
                _msg.InfoMessage($"Текущий режим обновления: {settings.Updates.Mode}");
                _msg.InfoMessage("Выберите режим: 0 — Disabled, 1 — CheckAndNotify, 2 — CheckAndInstall.");
                _msg.InfoMessage("Enter — оставить текущий режим.");

                string? input = Sys.Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    return;
                }

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
    }
}