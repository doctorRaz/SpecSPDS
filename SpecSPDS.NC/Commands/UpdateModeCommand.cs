using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Abstractions.Settings;
using dRz.Core.Services;
using dRz.NC.CadServices;
using dRz.Src.Infrastructure;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using Teigha.Runtime;
using System;
using System.Collections.Generic;

namespace dRz.NC.Commands
{
    /// <summary>
    /// Изменение режима и периода автоматического обновления через командную строку nanoCAD.
    /// </summary>
    public sealed class UpdateModeCommand
    {
        private static IDrzLogger _logger = AddOnContext.NLogFactory.GetLogger(typeof(UpdateModeCommand));

        private static IMessageService _msg = AddOnContext.Msg;

        private static IAddonSettings? _settings;

        private enum IntervalChoice
        {
            Yes,
            No
        }

        /// <summary>
        /// Показывает текущие настройки обновления и позволяет изменить их.
        /// Команда: drz_UpdateMode.
        /// </summary>
        [CommandMethod(CommandPrefix.Value + "drz_UpdateMode", CommandFlags.Session)]
        public static void SetUpdateMode()
        {
            Document? document = Application.DocumentManager.MdiActiveDocument;
            if (document == null)
            {
                return;
            }

            try
            {
                _settings = AddOnContext.AddonSettings;
            }
            catch (Exception ex)
            {
                const string message = "Не удалось получить настройки аддона:";

                _logger.Error(message, ex);
                _msg.ErrorMessage(message, ex);
                return;
            }

            IAddonSettings settings = _settings;
            IUpdateSettings updateSettings = settings.Updates;

            UpdateMode previousMode = updateSettings.Mode;
            int previousInterval = updateSettings.UpdateCheckIntervalDays;

            List<Keywords> modeKeywords = new List<Keywords>
            {
                new Keywords("Disabled", "Disabled", UpdateMode.Disabled, previousMode == UpdateMode.Disabled),
                new Keywords("Notify", "Notify", UpdateMode.CheckAndNotify, previousMode == UpdateMode.CheckAndNotify),
                new Keywords("Install", "Install", UpdateMode.CheckAndInstall, previousMode == UpdateMode.CheckAndInstall)
            };

            Enum? modeAnswer = CadService.KeywordAnswer(
                document,
                modeKeywords,
                $"\nТекущий режим: {GetDescription(previousMode)}. Новый режим: ",
                allowNone: true);

            if (modeAnswer == null)
            {
                return;
            }

            UpdateMode newMode = (UpdateMode)modeAnswer;

            // Период можно изменить независимо от режима, в том числе если режим оставлен прежним.
            List<Keywords> intervalKeywords = new List<Keywords>
            {
                new Keywords(nameof(IntervalChoice.Yes), "Yes", IntervalChoice.Yes),
                new Keywords(nameof(IntervalChoice.No), "No", IntervalChoice.No, isDefault: true)
            };

            Enum? intervalAnswer = CadService.KeywordAnswer(
                document,
                intervalKeywords,
                $"\nТекущий период проверки: {previousInterval} дн. Изменить период? ",
                allowNone: true);

            int newInterval = previousInterval;
            if (intervalAnswer is IntervalChoice.Yes)
            {
                PromptIntegerOptions daysOptions = new PromptIntegerOptions(
                    $"\nПериод автоматической проверки в днях (1–30) [{previousInterval}]: ")
                {
                    AllowNone = true,
                    DefaultValue = previousInterval,
                    LowerLimit = 1,
                    UpperLimit = 30,
                    AllowNegative = false,
                    AllowZero = false
                };

                PromptIntegerResult daysResult = document.Editor.GetInteger(daysOptions);
                if (daysResult.Status == PromptStatus.OK)
                {
                    newInterval = daysResult.Value;
                }
                // Отмена ввода периода не отменяет выбранный режим.
            }

            bool modeChanged = newMode != previousMode;
            bool intervalChanged = newInterval != previousInterval;

            if (!modeChanged && !intervalChanged)
            {
                _msg.InfoMessage("Настройки обновления не изменены.");
                return;
            }

            updateSettings.Mode = newMode;
            updateSettings.UpdateCheckIntervalDays = newInterval;

            try
            {
                settings.Save();

                if (modeChanged)
                {
                    _msg.InfoMessage($"Режим обновления сохранён: {GetDescription(newMode)}.");
                }

                if (intervalChanged)
                {
                    _msg.InfoMessage($"Период автоматической проверки сохранён: {newInterval} дн.");
                }
            }
            catch (Exception ex)
            {
                // При ошибке сохранения восстанавливаем обе настройки в памяти.
                updateSettings.Mode = previousMode;
                updateSettings.UpdateCheckIntervalDays = previousInterval;

                _msg.ErrorMessage(
                    "Не удалось сохранить настройки обновления. Восстановлены прежние значения.",
                    ex);
            }
        }

        private static string GetDescription(UpdateMode mode)
        {
            return mode switch
            {
                UpdateMode.Disabled => "отключено (обязательные обновления устанавливаются)",
                UpdateMode.CheckAndNotify => "проверка с подтверждением установки",
                UpdateMode.CheckAndInstall => "автоматическая установка необязательных обновлений",
                _ => mode.ToString()
            };
        }
    }
}
