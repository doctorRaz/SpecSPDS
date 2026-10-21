using dRz.Abstractions.Logger;
using dRz.Abstractions.Services.Message;
using dRz.Abstractions.Settings;
using dRz.Src.Infrastructure;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using Teigha.Runtime;
using Sys = System;

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
            catch (Sys.Exception ex)
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

            PromptKeywordOptions modeOptions = new PromptKeywordOptions(
                $"\nТекущий режим: {GetDescription(previousMode)}. Новый режим [{GetKeyword(previousMode)}]: ");
            modeOptions.Keywords.Add("Disabled");
            modeOptions.Keywords.Add("Notify");
            modeOptions.Keywords.Add("Install");
            modeOptions.Keywords.Default = GetKeyword(previousMode);
            modeOptions.AllowNone = true;

            PromptResult modeResult = document.Editor.GetKeywords(modeOptions);
            if (modeResult.Status == PromptStatus.Cancel)
            {
                return;
            }

            string selected = modeResult.Status == PromptStatus.None
                ? GetKeyword(previousMode)
                : modeResult.StringResult;

            UpdateMode newMode = selected.ToUpperInvariant() switch
            {
                "DISABLED" => UpdateMode.Disabled,
                "NOTIFY" => UpdateMode.CheckAndNotify,
                "INSTALL" => UpdateMode.CheckAndInstall,
                _ => previousMode
            };

            // Период можно изменить независимо от режима, в том числе если режим оставлен прежним.
            int newInterval = previousInterval;
            PromptKeywordOptions intervalOptions = new PromptKeywordOptions(
                $"\nТекущий период проверки: {previousInterval} дн. Изменить период? [Yes/No] <No>: ");
            intervalOptions.Keywords.Add("Yes");
            intervalOptions.Keywords.Add("No");
            intervalOptions.Keywords.Default = "No";
            intervalOptions.AllowNone = true;

            PromptResult intervalChoice = document.Editor.GetKeywords(intervalOptions);
            bool changeInterval = intervalChoice.Status != PromptStatus.Cancel
                && intervalChoice.Status != PromptStatus.Error
                && intervalChoice.Status != PromptStatus.Other
                && intervalChoice.Status != PromptStatus.None
                && string.Equals(intervalChoice.StringResult, "Yes", Sys.StringComparison.OrdinalIgnoreCase);

            if (changeInterval)
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
            catch (Sys.Exception ex)
            {
                // При ошибке сохранения восстанавливаем обе настройки в памяти.
                updateSettings.Mode = previousMode;
                updateSettings.UpdateCheckIntervalDays = previousInterval;

                _msg.ErrorMessage(
                    "Не удалось сохранить настройки обновления. Восстановлены прежние значения.",
                    ex);
            }
        }

        private static string GetKeyword(UpdateMode mode)
        {
            return mode switch
            {
                UpdateMode.Disabled => "Disabled",
                UpdateMode.CheckAndNotify => "Notify",
                UpdateMode.CheckAndInstall => "Install",
                _ => "Notify"
            };
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
