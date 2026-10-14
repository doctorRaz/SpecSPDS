using dRz.Abstractions.Settings;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using System;
using Teigha.Runtime;

namespace dRz.NC.Commands
{
    /// <summary>
    /// Изменение режима автоматического обновления через командную строку nanoCAD.
    /// </summary>
    public sealed class UpdateModeCommand
    {
        /// <summary>
        /// Показывает текущий режим и позволяет выбрать новый.
        /// Команда: drz_UpdateMode.
        /// </summary>
        [CommandMethod("drz_UpdateMode", CommandFlags.Session)]
        public static void SetUpdateMode()
        {
            Document? document = Application.DocumentManager.MdiActiveDocument;
            if (document == null)
                return;

            IAddonSettings settings;
            try
            {
                settings = AddOnCtx.Services.Get<IAddonSettings>();
            }
            catch (Exception ex)
            {
                document.Editor.WriteMessage($"\nНе удалось получить настройки аддона: {ex.Message}");
                return;
            }

            IUpdateSettings updateSettings = settings.Updates;
            UpdateMode previousMode = updateSettings.Mode;

            PromptKeywordOptions options = new PromptKeywordOptions(
                $"\nРежим обновления [{GetKeyword(previousMode)}]: ");
            options.Keywords.Add("Disabled");
            options.Keywords.Add("Notify");
            options.Keywords.Add("Install");
            options.Keywords.Default = GetKeyword(previousMode);
            options.AllowNone = true;

            PromptResult result = document.Editor.GetKeywords(options);
            if (result.Status == PromptStatus.Cancel)
                return;

            string selected = result.Status == PromptStatus.None
                ? GetKeyword(previousMode)
                : result.StringResult;

            UpdateMode newMode = selected switch
            {
                "Disabled" => UpdateMode.Disabled,
                "Notify" => UpdateMode.CheckAndNotify,
                "Install" => UpdateMode.CheckAndInstall,
                _ => previousMode
            };

            if (newMode == previousMode)
            {
                document.Editor.WriteMessage($"\nРежим обновления не изменён: {GetDescription(previousMode)}.");
                return;
            }

            updateSettings.Mode = newMode;
            try
            {
                settings.Save();
                document.Editor.WriteMessage($"\nРежим обновления сохранён: {GetDescription(newMode)}.");
            }
            catch (Exception ex)
            {
                // Если JSON не сохранился, возвращаем прежнее значение в памяти.
                updateSettings.Mode = previousMode;
                document.Editor.WriteMessage($"\nНе удалось сохранить режим обновления. Восстановлено прежнее значение: {GetDescription(previousMode)}. {ex.Message}");
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
