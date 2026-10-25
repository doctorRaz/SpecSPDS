using dRz.Abstractions.Infrastructure;
using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Abstractions.Settings;
using dRz.NC;
using dRz.NC.Infrastructure;
using System;
using System.Diagnostics;

using Sys = System;

namespace dRz.Test.Console.Updater
{
    public class FakeCAD
    {
        private readonly IDrzLogger _logger = null!;
        private readonly IMessageService _msg = null!;
        private readonly IAddonSettings _settings = null!;
        private readonly IAddOnInfo _addonInfo    = null!;
        private readonly Stopwatch _sw;

        /// <summary>Initializes a new instance of the <see cref="FakeCAD"/> class.</summary>
        public FakeCAD()
        {
            _sw = Stopwatch.StartNew();

            EntryPoint entryPoint = new();

            try
            {
                entryPoint.Initialize();

                _logger = AddOnContext.NLogFactory.GetLogger<FakeCAD>();

                _msg = AddOnContext.Msg;

                _addonInfo = AddOnContext.AddOnInfo;

                _msg.InfoMessage($"{nameof(_addonInfo.IsNotNetworkPath)}: {_addonInfo.IsNotNetworkPath.ToString()}");
                _settings = AddOnContext.AddonSettings;
            }
            catch (Exception ex)
            {
                _logger?.Fatal(ex, "Продолжение не возможно");

                if (_msg != null)
                {
                    _msg.ErrorMessage("Продолжение не возможно", ex);
                }
                else
                {
                    Sys.Console.Error.WriteLine($"Продолжение не возможно: {ex}");
                }
            }
            finally
            {
                entryPoint.Terminate();
                _logger?.Trace($"{nameof(FakeCAD)}: Terminate");
            }
        }

        //[STAThread]
        private static void Main(string[] args)
        {
            //AddOnRuntime.AddOnCompositionRoot root = ConteinerCtx.Run();

            //IAddOnInfo addInf = AddOnContext.AddOnInfo;

            FakeCAD fakeCad = new FakeCAD();

            fakeCad.SelectShowCommandInfoOnStartup();
            fakeCad.SelectUpdateMode();
            fakeCad.SelectUpdateCheckInterval();

            _ = ConsoleReadKey(fakeCad._msg, fakeCad._sw);
        }

        private static ConsoleKeyInfo ConsoleReadKey(IMessageService? msg, Stopwatch? sw = null)
        {
            string res = sw != null ? $"Total time {sw.Elapsed}\n" : string.Empty;
            string message = $"{res}Press any key to exit...";

            if (msg != null)
            {
                msg.InfoMessage(message);
            }
            else
            {
                Sys.Console.WriteLine(message);
            }

            return Sys.Console.ReadKey();
        }


        /// <summary>
        /// Позволяет включить или отключить вывод сведений о командах при запуске аддона.
        /// </summary>
        private void SelectShowCommandInfoOnStartup()
        {
            while (true)
            {
                bool currentValue = _settings.General.ShowCommandInfoOnStartup;
                _msg.InfoMessage($"Показывать сведения о командах при запуске: {(currentValue ? "Да" : "Нет")}");
                _msg.InfoMessage("Выберите: 1 — Да, 0 — Нет.");
                _msg.InfoMessage("Enter — оставить текущее значение.");

                string? input = Sys.Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    return;
                }

                bool selected;
                if (input.Trim() == "1")
                {
                    selected = true;
                }
                else if (input.Trim() == "0")
                {
                    selected = false;
                }
                else
                {
                    _msg.InfoMessage("Неверный выбор. Введите 0, 1 или Enter.");
                    continue;
                }

                _settings.General.ShowCommandInfoOnStartup = selected;

                try
                {
                    _settings.Save();
                    _msg.InfoMessage($"Вывод сведений о командах при запуске {(selected ? "включён" : "отключён")}.");
                }
                catch (Exception ex)
                {
                    _settings.General.ShowCommandInfoOnStartup = currentValue;
                    _logger?.Error(ex, "Не удалось сохранить настройку вывода сведений о командах.");
                    _msg.ErrorMessage("Не удалось сохранить настройку. Оставлено прежнее значение.", ex);
                }

                return;
            }
        }

        /// <summary>
        /// Позволяет вручную изменить период автоматической проверки обновлений.
        /// </summary>
        private void SelectUpdateCheckInterval()
        {
            while (true)
            {
                int currentInterval = _settings.Updates.UpdateCheckIntervalDays;
                _msg.InfoMessage($"Текущий период проверки обновлений: {currentInterval} дн.");
#if DEBUG
                const int minInterval = 0;
                _msg.InfoMessage("Введите период в днях (0–30; 0 — проверка при каждом запуске).");
#else
                const int minInterval = 1;
                _msg.InfoMessage("Введите период в днях (1–30).");
#endif
                _msg.InfoMessage("Enter — оставить текущий период.");

                string? input = Sys.Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    return;
                }

                if (!int.TryParse(input.Trim(), out int selected) ||
                    selected < minInterval ||
                    selected > 30)
                {
                    _msg.InfoMessage($"Неверный период. Допустимое значение: {minInterval}–30.");
                    continue;
                }

                _settings.Updates.UpdateCheckIntervalDays = selected;

                try
                {
                    _settings.Save();
                    _msg.InfoMessage($"Период проверки обновлений сохранён: {selected} дн.");
                }
                catch (Exception ex)
                {
                    _settings.Updates.UpdateCheckIntervalDays = currentInterval;
                    _logger?.Error(ex, "Не удалось сохранить период проверки обновлений.");
                    _msg.ErrorMessage("Не удалось сохранить период. Оставлен прежний период.", ex);
                }

                return;
            }
        }

        /// <summary>
        /// Позволяет вручную выбрать режим обновления для проверки в консольном тесте.
        /// </summary>
        private void SelectUpdateMode()
        {
            while (true)
            {
                _msg.InfoMessage($"Текущий режим обновления: {_settings.Updates.Mode}");
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

                UpdateMode previous = _settings.Updates.Mode;
                _settings.Updates.Mode = selected.Value;

                try
                {
                    _settings.Save();
                    _msg.InfoMessage($"Режим обновления сохранён: {_settings.Updates.Mode}");
                }
                catch (Exception ex)
                {
                    _settings.Updates.Mode = previous;
                    _logger?.Error(ex, "Не удалось сохранить режим обновления.");
                    _msg.ErrorMessage("Не удалось сохранить режим. Оставлен прежний режим.", ex);
                }

                return;
            }
        }
    }
}