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
    /// Изменение режима автоматического обновления через командную строку nanoCAD.
    /// </summary>
    public sealed class UpdateModeCommand
    {
        private static IDrzLogger _logger = AddOnContext.NLogFactory.GetLogger(typeof(UpdateModeCommand));

        private static IMessageService _msg = AddOnContext.Msg;

        private static IAddonSettings? _settings;

        /// <summary>
        /// Показывает текущий режим и позволяет выбрать новый.
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
                string msg = "Не удалось получить настройки аддона:";

                _logger.Error(msg, ex);

                _msg.ErrorMessage("Не удалось получить настройки аддона:", ex);

                return;
            }

            IUpdateSettings updateSettings = _settings.Updates;
            UpdateMode previousMode = updateSettings.Mode;

            PromptKeywordOptions options = new PromptKeywordOptions(
                                            $"\nТекущий режим: {GetDescription(previousMode)}. Новый режим [{GetKeyword(previousMode)}]: ");
            options.Keywords.Add("Disabled");
            options.Keywords.Add("Notify");
            options.Keywords.Add("Install");
            options.Keywords.Default = GetKeyword(previousMode);
            options.AllowNone = true;

            PromptResult result = document.Editor.GetKeywords(options);
            if (result.Status == PromptStatus.Cancel)
            {
                return;
            }

            string selected = result.Status == PromptStatus.None
                ? GetKeyword(previousMode)
                : result.StringResult;

            UpdateMode newMode = selected.ToUpperInvariant() switch
            {
                "DISABLED" => UpdateMode.Disabled,
                "NOTIFY" => UpdateMode.CheckAndNotify,
                "INSTALL" => UpdateMode.CheckAndInstall,
                _ => previousMode
            };

            if (newMode == previousMode)
            {
                _msg.InfoMessage($"Режим обновления не изменён: {GetDescription(previousMode)}.");

                return;
            }

            updateSettings.Mode = newMode;
            try
            {
                _settings.Save();
                _msg.InfoMessage($"Режим обновления сохранён: {GetDescription(newMode)}.");
            }
            catch (Sys.Exception ex)
            {
                // Если JSON не сохранился, возвращаем прежнее значение в памяти.
                updateSettings.Mode = previousMode;

                _msg.ErrorMessage($"Не удалось сохранить режим обновления.Восстановлено прежнее значение: {GetDescription(previousMode)}", ex);
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