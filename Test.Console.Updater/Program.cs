using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Abstractions.Settings;
using dRz.AddOnRuntime;
using dRz.NC;
using dRz.Updater.Services;
using System;
using System.Diagnostics;
using Sys = System;

namespace dRz.Test.Console.Updater
{
    public class Program
    {
        private readonly IDrzLogger _logger;
        private readonly IMessageService _msg;
        private readonly IAddonSettings _settings;

        public Program(
            IDrzLogger logger,
            IMessageService msg,
            IAddonSettings settings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _msg = msg ?? throw new ArgumentNullException(nameof(msg));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        //[STAThread]
        private static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            IDrzLogger? logger = null;
            IMessageService? msg = null;

            try
            {
                AddOnCompositionRoot root = ConteinerCtx.Run();

                logger = root.Get<IDrzLoggerFactory>().GetLogger<Program>();
                msg = root.Get<IMessageService>();
                IAddonSettings settings = root.Get<IAddonSettings>();

                Program program = new Program(logger, msg, settings);

                logger.Debug("Start debug updater");
                msg.InfoMessage("Start update");

                program.SelectUpdateMode();

                // UpdateAsync пока использует AddOnCtx внутри себя.
                // Инициализация этого контекста остаётся в ConteinerCtx как временный адаптер.
                UpdateAsync updateAsync = new UpdateAsync();
                _ = updateAsync.UpdateAsyncRun(UpdateLaunchMode.Automatic);
            }
            catch (Exception ex)
            {
                logger?.Fatal(ex, "Продолжение не возможно");

                if (msg != null)
                {
                    msg.ErrorMessage("Продолжение не возможно", ex);
                }
                else
                {
                    Sys.Console.Error.WriteLine($"Продолжение не возможно: {ex}");
                }
            }
            finally
            {
                logger?.Info("Terminate");
                _ = ConsoleReadKey(msg, sw);
            }
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
                    _logger.Error(ex, "Не удалось сохранить режим обновления.");
                    _msg.ErrorMessage("Не удалось сохранить режим. Оставлен прежний режим.", ex);
                }

                return;
            }
        }
    }
}